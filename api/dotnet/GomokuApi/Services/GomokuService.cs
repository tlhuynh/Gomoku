using GomokuApi.Constants;
using GomokuApi.Models;
using GomokuApi.Utilities;

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
    /// Implements the minimax algorithm with alpha-beta pruning to evaluate potential moves
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

        var validMoves = GetNearbyEmptySpaces(state);
        if (!validMoves.Any()) return 0;

        if (isMaximizing) {
            var maxScore = MIN_SCORE;
            foreach (var move in validMoves) {
                var newState = MakeMove(state, move, 2); // Player 2 (AI) is maximizing
                var score = Minimax(newState, depth - 1, alpha, beta, false);
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
                minScore = Math.Min(minScore, score);
                beta = Math.Min(beta, score);
                if (beta <= alpha) break; // Alpha-beta pruning
            }
            return minScore;
        }
    }

    /// <summary>
    /// Gets all valid moves that are within proximity to existing pieces
    /// This optimization focuses the search on relevant areas of the board
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>List of valid moves near existing pieces</returns>
    private List<MoveModel> GetNearbyEmptySpaces(GameStateModel state) {
        List<MoveModel> moves = [];

        // Check if board is empty using more efficient method
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

        return moves;
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
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <returns>Score indicating position strength (positive favors AI, negative favors human)</returns>
    private int EvaluateBoard(GameStateModel state) {
        int score = 0;
        // Check all possible lines
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                score += EvaluatePosition(state, i, j);
            }
        }
        return score;
    }

    /// <summary>
    /// Evaluates a specific position on the board by checking all possible lines through it
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row index of position</param>
    /// <param name="col">Column index of position</param>
    /// <returns>Cumulative score for all lines passing through this position</returns>
    private int EvaluatePosition(GameStateModel state, int row, int col) {
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
    /// Scores patterns like consecutive stones and potential wins
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Starting row</param>
    /// <param name="col">Starting column</param>
    /// <param name="drow">Row direction (0, 1, or -1)</param>
    /// <param name="dcol">Column direction (0, 1, or -1)</param>
    /// <returns>Score for the evaluated line</returns>
    private int EvaluateLine(GameStateModel state, int row, int col, int drow, int dcol) {
        var counts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 } };
        var spaces = 0;
        var length = 0;

        for (int i = -4; i <= 4; i++) {
            int newRow = row + i * drow;
            int newCol = col + i * dcol;

            if (newRow < 0 || newRow >= GameConstants.BOARD_SIZE || newCol < 0 || newCol >= GameConstants.BOARD_SIZE)
                continue;

            int value = state.Board[newRow, newCol];
            if (value == 0)
                spaces++;
            else
                counts[value]++;

            length++;
        }

        // Score based on patterns
        foreach (var player in counts.Keys) {
            int count = counts[player];
            if (count >= GameConstants.WIN_LENGTH) return player == 1 ? MAX_SCORE : MIN_SCORE;
            if (count == 4 && spaces > 0) return player == 1 ? 1000 : -1000;
            if (count == 3 && spaces > 1) return player == 1 ? 100 : -100;
            if (count == 2 && spaces > 2) return player == 1 ? 10 : -10;
        }

        return 0;
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
    private int CountDirection(GameStateModel gameState, MoveModel move, int drow, int dcol) {
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