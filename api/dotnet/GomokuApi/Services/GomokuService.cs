using GomokuApi.Models;

namespace GomokuApi.Services;

public interface IGomokuService {
    Move GetBestMove(GameState gameState, int depth = 3);
    bool IsValidMove(GameState gameState, Move move);
    bool CheckWin(GameState gameState, Move lastMove);
    GameState MakeMove(GameState gameState, Move move);
}

public class GomokuService : IGomokuService {
    private const int BOARD_SIZE = 15;
    private const int WIN_LENGTH = 5;
    private const int MAX_SCORE = 1000000;
    private const int MIN_SCORE = -1000000;

    public Move GetBestMove(GameState gameState, int depth = 3) {
        var bestScore = MIN_SCORE;
        Move bestMove = null;

        // Get all valid moves within 2 spaces of existing pieces
        var validMoves = GetNearbyEmptySpaces(gameState);

        foreach (var move in validMoves) {
            var newState = MakeMove(gameState, move);
            var score = Minimax(newState, depth - 1, MIN_SCORE, MAX_SCORE, false);

            if (score > bestScore) {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove ?? validMoves.FirstOrDefault();
    }

    private int Minimax(GameState state, int depth, int alpha, int beta, bool isMaximizing) {
        // Terminal conditions
        if (depth == 0) return EvaluateBoard(state);

        var validMoves = GetNearbyEmptySpaces(state);
        if (!validMoves.Any()) return 0;

        if (isMaximizing) {
            var maxScore = MIN_SCORE;
            foreach (var move in validMoves) {
                var newState = MakeMove(state, move);
                var score = Minimax(newState, depth - 1, alpha, beta, false);
                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);
                if (beta <= alpha) break; // Alpha-beta pruning
            }
            return maxScore;
        } else {
            var minScore = MAX_SCORE;
            foreach (var move in validMoves) {
                var newState = MakeMove(state, move);
                var score = Minimax(newState, depth - 1, alpha, beta, true);
                minScore = Math.Min(minScore, score);
                beta = Math.Min(beta, score);
                if (beta <= alpha) break; // Alpha-beta pruning
            }
            return minScore;
        }
    }

    private List<Move> GetNearbyEmptySpaces(GameState state) {
        var moves = new List<Move>();
        var hasAnyPieces = false;

        // Check if board is empty
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                if (state.Board[i, j] != 0) {
                    hasAnyPieces = true;
                    break;
                }
            }
            if (hasAnyPieces) break;
        }

        // If board is empty, return center position
        if (!hasAnyPieces) {
            return new List<Move> { new Move { Row = BOARD_SIZE / 2, Col = BOARD_SIZE / 2, Player = state.CurrentPlayer } };
        }

        // Find empty spaces near existing pieces
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                if (state.Board[i, j] == 0 && HasAdjacentPiece(state, i, j)) {
                    moves.Add(new Move { Row = i, Col = j, Player = state.CurrentPlayer });
                }
            }
        }

        return moves;
    }

    private bool HasAdjacentPiece(GameState state, int row, int col, int distance = 2) {
        for (int i = Math.Max(0, row - distance); i <= Math.Min(BOARD_SIZE - 1, row + distance); i++) {
            for (int j = Math.Max(0, col - distance); j <= Math.Min(BOARD_SIZE - 1, col + distance); j++) {
                if (state.Board[i, j] != 0)
                    return true;
            }
        }
        return false;
    }

    private int EvaluateBoard(GameState state) {
        int score = 0;
        // Check all possible lines
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                score += EvaluatePosition(state, i, j);
            }
        }
        return score;
    }

    private int EvaluatePosition(GameState state, int row, int col) {
        int[][] directions = {
            new[] { 1, 0 },   // vertical
            new[] { 0, 1 },   // horizontal
            new[] { 1, 1 },   // diagonal
            new[] { 1, -1 }   // anti-diagonal
        };

        int score = 0;
        foreach (var dir in directions) {
            score += EvaluateLine(state, row, col, dir[0], dir[1]);
        }
        return score;
    }

    private int EvaluateLine(GameState state, int row, int col, int drow, int dcol) {
        var counts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 } };
        var spaces = 0;
        var length = 0;

        for (int i = -4; i <= 4; i++) {
            int newRow = row + i * drow;
            int newCol = col + i * dcol;

            if (newRow < 0 || newRow >= BOARD_SIZE || newCol < 0 || newCol >= BOARD_SIZE)
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
            if (count >= WIN_LENGTH) return player == 1 ? MAX_SCORE : MIN_SCORE;
            if (count == 4 && spaces > 0) return player == 1 ? 1000 : -1000;
            if (count == 3 && spaces > 1) return player == 1 ? 100 : -100;
            if (count == 2 && spaces > 2) return player == 1 ? 10 : -10;
        }

        return 0;
    }

    public bool IsValidMove(GameState gameState, Move move) {
        if (move.Row < 0 || move.Row >= BOARD_SIZE || move.Col < 0 || move.Col >= BOARD_SIZE)
            return false;

        return gameState.Board[move.Row, move.Col] == 0;
    }

    public bool CheckWin(GameState gameState, Move lastMove) {
        if (lastMove == null) return false;

        int[][] directions = {
            new[] { 1, 0 },   // vertical
            new[] { 0, 1 },   // horizontal
            new[] { 1, 1 },   // diagonal
            new[] { 1, -1 }   // anti-diagonal
        };

        foreach (var dir in directions) {
            int count = 1;
            count += CountDirection(gameState, lastMove, dir[0], dir[1]);
            count += CountDirection(gameState, lastMove, -dir[0], -dir[1]);

            if (count >= WIN_LENGTH)
                return true;
        }

        return false;
    }

    private int CountDirection(GameState gameState, Move move, int drow, int dcol) {
        int count = 0;
        int row = move.Row + drow;
        int col = move.Col + dcol;
        int player = move.Player;

        while (row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE &&
               gameState.Board[row, col] == player) {
            count++;
            row += drow;
            col += dcol;
        }

        return count;
    }

    public GameState MakeMove(GameState gameState, Move move) {
        if (!IsValidMove(gameState, move))
            return gameState;

        var newState = new GameState {
            Board = (int[,])gameState.Board.Clone(),
            CurrentPlayer = gameState.CurrentPlayer,
            IsGameOver = gameState.IsGameOver,
            Winner = gameState.Winner
        };

        newState.Board[move.Row, move.Col] = move.Player;

        if (CheckWin(newState, move)) {
            newState.IsGameOver = true;
            newState.Winner = move.Player;
        } else {
            newState.CurrentPlayer = move.Player == 1 ? 2 : 1;
        }

        return newState;
    }
}