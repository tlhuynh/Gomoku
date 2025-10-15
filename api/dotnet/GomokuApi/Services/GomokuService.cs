using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services;

/// <summary>
/// Interface for the Gomoku game service, providing core game mechanics and AI logic
/// </summary>
public interface IGomokuService {
    /// <summary>
    /// Determines the best move for the AI player using minimax algorithm with alpha-beta pruning
    /// </summary>
    /// <param name="gameState">Current state of the game board and player information</param>
    /// <returns>The best move determined by the AI</returns>
    MoveModel GetBestMove(GameStateModel gameState);

    /// <summary>
    /// Checks if a move is valid according to game rules
    /// </summary>
    /// <param name="gameState">Current state of the game</param>
    /// <param name="move">The move to validate</param>
    /// <returns>True if the move is valid, false otherwise</returns>
    bool IsValidMove(GameStateModel gameState, MoveModel move);

    /// <summary>
    /// Applies a move to the game state and returns the new resulting state
    /// </summary>
    /// <param name="gameState">Current state of the game</param>
    /// <param name="move">The move to apply</param>
    /// <param name="playerToMove">Player to make the move</param>
    /// <returns>New game state after the move is applied</returns>
    GameStateModel MakeMove(GameStateModel gameState, MoveModel move, int playerToMove);

    /// <summary>
    /// Checks if the last move created a winning condition (5 or more stones in a row)
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="lastMove">The most recent move that was made</param>
    /// <returns>True if the move created a winning condition, false otherwise</returns>
    public bool CheckWin(GameStateModel gameState, MoveModel lastMove);
}

/// <summary>
/// Implementation of the Gomoku game service providing AI gameplay and game mechanics
/// </summary>
public class GomokuService : IGomokuService {
    /// <summary>
    /// Maximum possible evaluation score for a winning position
    /// </summary>
    private const int MAX_SCORE = 1000000;

    /// <summary>
    /// Minimum possible evaluation score for a losing position
    /// </summary>
    private const int MIN_SCORE = -1000000;

    /// <summary>
    /// Uses minimax algorithm to determine the best move for the AI player
    /// </summary>
    /// <param name="gameState">Current state of the game board</param>
    /// <returns>The best move for the current player</returns>
    public MoveModel GetBestMove(GameStateModel gameState) {
        // Determine search depth based on difficulty
        int depth = (int)gameState.Difficulty;
        int bestScore = MIN_SCORE;
        MoveModel? bestMove = null;

        // Reduce depth in early game to improve performance
        int numberOfPieces = gameState.Board.Cast<int>().Count(cell => cell != 0);
        // Adjust depth based on game stage
        if (numberOfPieces < 5) {
            // Early game - reduce depth
            depth = Math.Min(depth, 2);
        }

        // Get all valid moves within 2 spaces of existing pieces
        List<MoveModel> validMoves = GetNearbyEmptySpaces(gameState);

        foreach (var move in validMoves) {
            var newState = MakeMove(gameState, move, 2); // AI is making this move (player 2)
            var score = Minimax(newState, depth - 1, MIN_SCORE, MAX_SCORE, false);

            if (score > bestScore) {
                bestScore = score;
                bestMove = move;
            }
        }

        // If no best move was found or there are no valid moves, return center or first available move
        return bestMove ?? validMoves.FirstOrDefault() ?? new MoveModel {
            Row = GameConstants.BOARD_SIZE / 2,
            Col = GameConstants.BOARD_SIZE / 2
        };
    }

    /// <summary>
    /// Implements the minimax algorithm with alpha-beta pruning and optimized move ordering
    /// </summary>
    /// <param name="state">Current game state to evaluate</param>
    /// <param name="depth">Remaining search depth</param>
    /// <param name="alpha">Alpha value for pruning</param>
    /// <param name="beta">Beta value for pruning</param>
    /// <param name="isMaximizing">Whether this is a maximizing turn (AI) or minimizing turn (opponent)</param>
    /// <returns>The evaluation score for the current state</returns>
    private int Minimax(GameStateModel state, int depth, int alpha, int beta, bool isMaximizing) {
        // Terminal conditions
        if (depth == 0) return EvaluateBoard(state);

        // Check for win/loss conditions to prioritize immediate threats
        var (aiWinningMove, humanWinningMove) = DetectImmediateThreats(state);
        if (aiWinningMove) return MAX_SCORE - (5 - depth); // Prefer quicker wins
        if (humanWinningMove) return MIN_SCORE + (5 - depth); // Prefer blocking immediate threats

        var validMoves = GetNearbyEmptySpaces(state);
        if (validMoves.Count == 0) return 0;

        // Evaluate immediately critical moves first to improve pruning
        validMoves = PrioritizeCriticalMoves(state, validMoves, isMaximizing);

        if (isMaximizing) {
            var maxScore = MIN_SCORE;
            foreach (var move in validMoves) {
                var newState = MakeMove(state, move, 2); // Player 2 (AI) is maximizing
                var score = Minimax(newState, depth - 1, alpha, beta, false);

                // If this move wins, no need to search further
                if (score >= MAX_SCORE - 10) return score - 1; // Slight adjustment for depth

                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);
                if (beta <= alpha) break; // Alpha-beta pruning
            }
            return maxScore;
        } else {
            var minScore = MAX_SCORE;
            foreach (var move in validMoves) {
                var newState = MakeMove(state, move, 1); // Player 1 (Human) is minimizing
                var score = Minimax(newState, depth - 1, alpha, beta, true);

                // If this move wins for human, no need to search further
                if (score <= MIN_SCORE + 10) return score + 1; // Slight adjustment for depth

                minScore = Math.Min(minScore, score);
                beta = Math.Min(beta, score);
                if (beta <= alpha) break; // Alpha-beta pruning
            }
            return minScore;
        }
    }

    /// <summary>
    /// Prioritizes critical moves for more efficient alpha-beta pruning
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="moves">Valid moves to consider</param>
    /// <param name="isMaximizing">Whether this is the maximizing player's turn</param>
    /// <returns>Prioritized list of moves to evaluate</returns>
    private static List<MoveModel> PrioritizeCriticalMoves(GameStateModel state, List<MoveModel> moves, bool isMaximizing) {
        // Clone the moves list to avoid modifying the original
        var prioritizedMoves = new List<MoveModel>(moves);
        var criticalMoves = new List<MoveModel>();

        int currentPlayer = isMaximizing ? 2 : 1;

        // First identify moves that win immediately
        foreach (var move in moves) {
            var tempState = new GameStateModel {
                Board = (int[,])state.Board.Clone(),
                Difficulty = state.Difficulty
            };

            tempState.Board[move.Row, move.Col] = currentPlayer;

            // Check if this creates a win for current player
            if (CheckWinAtPosition(tempState, move.Row, move.Col)) {
                criticalMoves.Add(move);
            }

            // Reset for next check
            tempState.Board[move.Row, move.Col] = 0;
        }

        // If we found winning moves, only return those
        if (criticalMoves.Count > 0) return criticalMoves;

        // Next, check for moves that block opponent's win
        int opponent = currentPlayer == 1 ? 2 : 1;
        foreach (var move in moves) {
            var tempState = new GameStateModel {
                Board = (int[,])state.Board.Clone(),
                Difficulty = state.Difficulty
            };

            tempState.Board[move.Row, move.Col] = opponent;

            // Check if this would create a win for opponent
            if (CheckWinAtPosition(tempState, move.Row, move.Col)) {
                criticalMoves.Add(move);
            }
        }

        // If we found blocking moves, prioritize those but include others
        if (criticalMoves.Count > 0) {
            // Remove the critical moves from prioritizedMoves
            foreach (var move in criticalMoves) {
                prioritizedMoves.RemoveAll(m => m.Row == move.Row && m.Col == move.Col);
            }

            // Add them back at the beginning
            prioritizedMoves.InsertRange(0, criticalMoves);
            return prioritizedMoves;
        }

        // If no critical moves, return the original ordered list
        return moves;
    }

    /// <summary>
    /// Gets all valid moves that are within proximity to existing pieces
    /// This optimization focuses the search on relevant areas of the board
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>List of valid moves near existing pieces</returns>
    private static List<MoveModel> GetNearbyEmptySpaces(GameStateModel state) {
        List<MoveModel> moves = [];

        // Check if board is empty
        if (state.Board.Cast<int>().All(cell => cell == 0)) {
            // If board is empty, return center position
            return [new MoveModel {
                Row = GameConstants.BOARD_SIZE / 2,
                Col = GameConstants.BOARD_SIZE / 2
            }];
        }

        // Find empty spaces near existing pieces
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] == 0 && HasAdjacentPiece(state, i, j)) {
                    moves.Add(new MoveModel { Row = i, Col = j });
                }
            }
        }

        // Sort moves by potential (evaluate each move with a quick heuristic)
        return [.. moves.OrderByDescending(m => QuickEvaluate(state, m))];
    }

    /// <summary>
    /// Quick heuristic evaluation for a move based on proximity to center and existing pieces
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="move">Proposed move</param>
    /// <returns>Heuristic score for the move</returns>
    private static int QuickEvaluate(GameStateModel state, MoveModel move) {
        // Simple heuristic - prefer center and positions near existing pieces
        int centerDistance = Math.Abs(move.Row - GameConstants.BOARD_SIZE / 2) +
                             Math.Abs(move.Col - GameConstants.BOARD_SIZE / 2);

        // Count nearby pieces (indicates tactically important area)
        int nearbyPieces = CountAdjacentPieces(state, move.Row, move.Col);

        // Combine factors - center proximity and tactical importance
        return GameConstants.BOARD_SIZE - centerDistance + (nearbyPieces * 2);
    }

    /// <summary>
    /// Counts the number of adjacent pieces around a given position
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row index to check</param>
    /// <param name="col">Column index to check</param>
    /// <returns>Number of adjacent pieces</returns>
    private static int CountAdjacentPieces(GameStateModel state, int row, int col) {
        int count = 0;
        for (int i = Math.Max(0, row - 2); i <= Math.Min(GameConstants.BOARD_SIZE - 1, row + 2); i++) {
            for (int j = Math.Max(0, col - 2); j <= Math.Min(GameConstants.BOARD_SIZE - 1, col + 2); j++) {
                if (state.Board[i, j] != 0) count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Checks if a position has any adjacent pieces within the specified distance
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row index to check</param>
    /// <param name="col">Column index to check</param>
    /// <param name="distance">Maximum distance to check for adjacent pieces</param>
    /// <returns>True if there is at least one piece within the specified distance</returns>
    private static bool HasAdjacentPiece(GameStateModel state, int row, int col, int distance = 2) {
        for (int i = Math.Max(0, row - distance); i <= Math.Min(GameConstants.BOARD_SIZE - 1, row + distance); i++) {
            for (int j = Math.Max(0, col - distance); j <= Math.Min(GameConstants.BOARD_SIZE - 1, col + distance); j++) {
                if (state.Board[i, j] != 0)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Evaluates the current game board to determine relative strength of position
    /// Uses comprehensive scoring including pattern recognition, board control, and threat assessment
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>Score indicating position strength (positive favors AI, negative favors human)</returns>
    private int EvaluateBoard(GameStateModel state) {
        // First check for immediate winning and blocking moves
        var (aiWinningMove, humanWinningMove) = DetectImmediateThreats(state);
        if (aiWinningMove) return MAX_SCORE;
        if (humanWinningMove) return MIN_SCORE;

        int score = 0;

        // Pattern evaluation - check all possible lines
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                score += EvaluatePosition(state, i, j);
            }
        }

        // Board control evaluation - favor controlling the center and key areas
        score += EvaluateBoardControl(state);

        // Stone development evaluation - assess how well stones are connected
        score += EvaluateStoneDevelopment(state);

        return score;
    }

    /// <summary>
    /// Evaluates board control by analyzing stone positions and strategic areas
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>Score reflecting board control advantage</returns>
    private static int EvaluateBoardControl(GameStateModel state) {
        int score = 0;
        int center = GameConstants.BOARD_SIZE / 2;

        // Evaluate control of the center area (3x3 center area has more value)
        for (int i = center - 1; i <= center + 1; i++) {
            for (int j = center - 1; j <= center + 1; j++) {
                if (i >= 0 && i < GameConstants.BOARD_SIZE && j >= 0 && j < GameConstants.BOARD_SIZE) {
                    if (state.Board[i, j] == 2) score += 30; // AI controls center
                    else if (state.Board[i, j] == 1) score -= 30; // Human controls center
                }
            }
        }

        // Evaluate control of the middle ring around center (5x5 excluding center)
        for (int i = center - 2; i <= center + 2; i++) {
            for (int j = center - 2; j <= center + 2; j++) {
                if (i >= 0 && i < GameConstants.BOARD_SIZE && j >= 0 && j < GameConstants.BOARD_SIZE) {
                    // Skip the center 3x3 (already counted)
                    if (Math.Abs(i - center) <= 1 && Math.Abs(j - center) <= 1) continue;

                    if (state.Board[i, j] == 2) score += 15; // AI controls middle ring
                    else if (state.Board[i, j] == 1) score -= 15; // Human controls middle ring
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Evaluates how well stones are connected and developed
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>Score reflecting stone development advantage</returns>
    private static int EvaluateStoneDevelopment(GameStateModel state) {
        int score = 0;

        // Check for connected stones (adjacent pairs)
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] == 0) continue;

                int player = state.Board[i, j];
                int connectionScore = player == 2 ? 5 : -5;

                // Check all 8 adjacent directions
                int[][] directions = [
                    [1, 0], [1, 1], [0, 1], [-1, 1],
                    [-1, 0], [-1, -1], [0, -1], [1, -1]
                ];

                foreach (var dir in directions) {
                    int ni = i + dir[0];
                    int nj = j + dir[1];

                    if (ni >= 0 && ni < GameConstants.BOARD_SIZE &&
                        nj >= 0 && nj < GameConstants.BOARD_SIZE &&
                        state.Board[ni, nj] == player) {
                        // Connected stones found
                        score += connectionScore;
                    }
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Detects immediate winning threats and blocking opportunities
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>Information about immediate threats</returns>
    private static (bool aiWinningMove, bool humanWinningMove) DetectImmediateThreats(GameStateModel state) {
        bool aiWinningMove = false;
        bool humanWinningMove = false;

        // Check for immediate winning patterns (four in a row with open end)
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] != 0) continue;

                // Try placing AI stone
                var tempState = new GameStateModel {
                    Board = (int[,])state.Board.Clone(),
                    Difficulty = state.Difficulty
                };
                tempState.Board[i, j] = 2;

                // Check if this creates 5 in a row for AI
                if (CheckWinAtPosition(tempState, i, j)) {
                    aiWinningMove = true;
                    break;
                }

                // Try placing human stone
                tempState.Board[i, j] = 1;

                // Check if this creates 5 in a row for human
                if (CheckWinAtPosition(tempState, i, j)) {
                    humanWinningMove = true;
                }

                // Reset
                tempState.Board[i, j] = 0;
            }

            if (aiWinningMove) break;
        }

        return (aiWinningMove, humanWinningMove);
    }

    /// <summary>
    /// Checks if a particular position creates a winning position
    /// </summary>
    /// <param name="state">Game state to check</param>
    /// <param name="row">Row position</param>
    /// <param name="col">Column position</param>
    /// <returns>True if this position creates a win</returns>
    private static bool CheckWinAtPosition(GameStateModel state, int row, int col) {
        int player = state.Board[row, col];
        if (player == 0) return false;

        int[][] directions = [
            [1, 0],   // vertical
            [0, 1],   // horizontal
            [1, 1],   // diagonal
            [1, -1]   // anti-diagonal
        ];

        foreach (var dir in directions) {
            int count = 1; // Start with 1 for the current position

            // Count in positive direction
            for (int i = 1; i < 5; i++) {
                int newRow = row + i * dir[0];
                int newCol = col + i * dir[1];

                if (newRow < 0 || newRow >= GameConstants.BOARD_SIZE ||
                    newCol < 0 || newCol >= GameConstants.BOARD_SIZE ||
                    state.Board[newRow, newCol] != player) {
                    break;
                }

                count++;
            }

            // Count in negative direction
            for (int i = 1; i < 5; i++) {
                int newRow = row - i * dir[0];
                int newCol = col - i * dir[1];

                if (newRow < 0 || newRow >= GameConstants.BOARD_SIZE ||
                    newCol < 0 || newCol >= GameConstants.BOARD_SIZE ||
                    state.Board[newRow, newCol] != player) {
                    break;
                }

                count++;
            }

            if (count >= 5) return true;
        }

        return false;
    }

    /// <summary>
    /// Evaluates a specific position on the board by checking all possible lines through it
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row index of position</param>
    /// <param name="col">Column index of position</param>
    /// <returns>Cumulative score for all lines passing through this position</returns>
    private static int EvaluatePosition(GameStateModel state, int row, int col) {
        int[][] directions = [
            [1, 0],   // vertical
            [0, 1],   // horizontal
            [1, 1],   // diagonal
            [1, -1]   // anti-diagonal
        ];

        int score = 0;
        foreach (var dir in directions) {
            score += EvaluateLine(state, row, col, dir[0], dir[1]);
        }
        return score;
    }

    /// <summary>
    /// Evaluates a line in the specified direction from a given position
    /// Uses advanced pattern recognition to score different stone configurations
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Starting row</param>
    /// <param name="col">Starting column</param>
    /// <param name="drow">Row direction (0, 1, or -1)</param>
    /// <param name="dcol">Column direction (0, 1, or -1)</param>
    /// <returns>Score for the evaluated line</returns>
    private static int EvaluateLine(GameStateModel state, int row, int col, int drow, int dcol) {
        // Extract the line to analyze (9 positions: 4 on each side + center)
        var line = new int[9];
        var validPositions = 0;

        for (int i = -4; i <= 4; i++) {
            int newRow = row + i * drow;
            int newCol = col + i * dcol;

            // Default to -1 for out of bounds (to distinguish from empty spaces which are 0)
            int value = -1;

            if (newRow >= 0 && newRow < GameConstants.BOARD_SIZE &&
                newCol >= 0 && newCol < GameConstants.BOARD_SIZE) {
                value = state.Board[newRow, newCol];
                validPositions++;
            }

            line[i + 4] = value;
        }

        // Not enough valid positions to form any significant pattern
        if (validPositions < 5) return 0;

        int totalScore = 0;

        // Analyze patterns for both players
        totalScore += AnalyzePatterns(line, 2); // AI player patterns
        totalScore -= AnalyzePatterns(line, 1); // Human player patterns

        return totalScore;
    }

    /// <summary>
    /// Analyzes patterns in a line for a specific player
    /// </summary>
    /// <param name="line">The line to analyze</param>
    /// <param name="player">Player to analyze patterns for</param>
    /// <returns>Score based on patterns detected</returns>
    private static int AnalyzePatterns(int[] line, int player) {
        int score = 0;

        // Check for winning pattern (five or more in a row)
        if (ContainsConsecutive(line, player, 5))
            return player == 2 ? MAX_SCORE : MIN_SCORE;

        // Check for open four (extremely strong threat)
        // Pattern: 0PPPP0 (where P is player stone)
        if (ContainsPattern(line, player, [0, player, player, player, player, 0]))
            return player == 2 ? 10000 : -10000;

        // Check for multiple open threes (very strong position)
        int openThrees = CountPattern(line, player, [0, player, player, player, 0]);
        if (openThrees >= 2)
            return player == 2 ? 5000 : -5000;
        else if (openThrees == 1)
            return player == 2 ? 1000 : -1000;

        // Half-open four (strong threat, but can be blocked)
        // Pattern: PPPP0 or 0PPPP or PP0PP
        if (ContainsPattern(line, player, [player, player, player, player, 0]) ||
            ContainsPattern(line, player, [0, player, player, player, player]) ||
            ContainsPattern(line, player, [player, player, 0, player, player]))
            score += player == 2 ? 800 : -800;

        // Potential threes (developing threats)
        if (ContainsPattern(line, player, [0, player, player, 0, 0]) ||
            ContainsPattern(line, player, [0, 0, player, player, 0]) ||
            ContainsPattern(line, player, [0, player, 0, player, 0]))
            score += player == 2 ? 300 : -300;

        // Connected pairs (building blocks)
        if (ContainsPattern(line, player, [0, 0, player, player, 0, 0]))
            score += player == 2 ? 100 : -100;

        // Single stones with space (initial development)
        int singleStones = 0;
        for (int i = 0; i < line.Length; i++) {
            if (line[i] == player) singleStones++;
        }
        score += singleStones * (player == 2 ? 10 : -10);

        return score;
    }

    /// <summary>
    /// Checks if a line contains a specific pattern
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <param name="player">The player's stone to match in the pattern</param>
    /// <param name="pattern">The pattern to look for</param>
    /// <returns>True if pattern is found, false otherwise</returns>
    private static bool ContainsPattern(int[] line, int player, int[] pattern) {
        // Slide the pattern window across the line
        for (int i = 0; i <= line.Length - pattern.Length; i++) {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++) {
                // Skip matching -1 (out of bounds)
                if (line[i + j] == -1) {
                    match = false;
                    break;
                }

                if (pattern[j] != 0 && line[i + j] != pattern[j]) {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }
        return false;
    }

    /// <summary>
    /// Counts occurrences of a specific pattern in a line
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <param name="player">The player's stone to match</param>
    /// <param name="pattern">The pattern to count</param>
    /// <returns>Number of occurrences of the pattern</returns>
    private static int CountPattern(int[] line, int player, int[] pattern) {
        int count = 0;
        // Slide the pattern window across the line
        for (int i = 0; i <= line.Length - pattern.Length; i++) {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++) {
                // Skip matching -1 (out of bounds)
                if (line[i + j] == -1) {
                    match = false;
                    break;
                }

                if (pattern[j] != 0 && line[i + j] != pattern[j]) {
                    match = false;
                    break;
                }
            }
            if (match) count++;
        }
        return count;
    }

    /// <summary>
    /// Checks if a line contains at least n consecutive stones of a player
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <param name="player">The player's stone to check for</param>
    /// <param name="n">Number of consecutive stones to look for</param>
    /// <returns>True if pattern is found, false otherwise</returns>
    private static bool ContainsConsecutive(int[] line, int player, int n) {
        int consecutive = 0;
        for (int i = 0; i < line.Length; i++) {
            if (line[i] == player) {
                consecutive++;
                if (consecutive >= n) return true;
            } else {
                consecutive = 0;
            }
        }
        return false;
    }

    /// <summary>
    /// Verifies if a move is valid according to game rules
    /// A move is valid if it's within the board boundaries and targets an empty cell
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="move">The move to validate</param>
    /// <returns>True if the move is valid, false otherwise</returns>
    public bool IsValidMove(GameStateModel gameState, MoveModel move) {
        if (move.Row < 0 || move.Row >= GameConstants.BOARD_SIZE || move.Col < 0 || move.Col >= GameConstants.BOARD_SIZE)
            return false;

        return gameState.Board[move.Row, move.Col] == 0;
    }

    /// <summary>
    /// Checks if the last move created a winning condition (5 or more stones in a row)
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="lastMove">The most recent move that was made</param>
    /// <returns>True if the move created a winning condition, false otherwise</returns>
    public bool CheckWin(GameStateModel gameState, MoveModel lastMove) {
        if (lastMove == null) return false;

        int[][] directions = [
            [1, 0],   // vertical
            [0, 1],   // horizontal
            [1, 1],   // diagonal
            [1, -1]   // anti-diagonal
        ];

        foreach (var dir in directions) {
            int count = 1;
            count += CountDirection(gameState, lastMove, dir[0], dir[1]);
            count += CountDirection(gameState, lastMove, -dir[0], -dir[1]);

            if (count >= GameConstants.WIN_LENGTH)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Counts consecutive stones in a given direction from a starting position
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="move">The starting move position</param>
    /// <param name="drow">Row direction (0, 1, or -1)</param>
    /// <param name="dcol">Column direction (0, 1, or -1)</param>
    /// <returns>Count of consecutive stones of the same player in the specified direction</returns>
    private static int CountDirection(GameStateModel gameState, MoveModel move, int drow, int dcol) {
        int count = 0;
        int row = move.Row + drow;
        int col = move.Col + dcol;
        // Get the player from the position of the last move
        int player = gameState.Board[move.Row, move.Col];

        while (row >= 0 && row < GameConstants.BOARD_SIZE && col >= 0 && col < GameConstants.BOARD_SIZE &&
               gameState.Board[row, col] == player) {
            count++;
            row += drow;
            col += dcol;
        }

        return count;
    }

    /// <summary>
    /// Applies a move to the current game state and returns the resulting new state
    /// Also checks for win conditions and updates game status accordingly
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="move">The move to apply</param>
    /// <param name="playerToMove">Player to make the move</param>
    /// <returns>New game state after the move is applied, or original state if move is invalid</returns>
    public GameStateModel MakeMove(GameStateModel gameState, MoveModel move, int playerToMove) {
        if (!IsValidMove(gameState, move))
            return gameState;

        var newState = new GameStateModel {
            Board = (int[,])gameState.Board.Clone(),
            Difficulty = gameState.Difficulty
        };

        // In GetBestMove, this will be the AI (player 2)
        // In Minimax, this will alternate between players based on isMaximizing
        newState.Board[move.Row, move.Col] = playerToMove;
        return newState;
    }
}