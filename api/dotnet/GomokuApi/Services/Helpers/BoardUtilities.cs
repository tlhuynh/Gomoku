using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services.Helpers;

/// <summary>
/// Utility methods for board operations and analysis
/// </summary>
public static class BoardUtilities {
    /// <summary>
    /// Gets a line of cells in a specific direction from a given position
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Starting row</param>
    /// <param name="col">Starting column</param>
    /// <param name="drow">Row direction</param>
    /// <param name="dcol">Column direction</param>
    /// <returns>Array of cell values in the line</returns>
    public static int[] GetLineAt(GameStateModel state, int row, int col, int drow, int dcol) {
        var line = new int[GameConstants.CacheSettings.LINE_LENGTH];

        for (int i = -GameConstants.CacheSettings.LINE_CENTER; i <= GameConstants.CacheSettings.LINE_CENTER; i++) {
            int newRow = row + i * drow;
            int newCol = col + i * dcol;

            int value = -1; // Out of bounds marker
            if (newRow >= 0 && newRow < GameConstants.BOARD_SIZE &&
                newCol >= 0 && newCol < GameConstants.BOARD_SIZE) {
                value = state.Board[newRow, newCol];
            }

            line[i + GameConstants.CacheSettings.LINE_CENTER] = value;
        }

        return line;
    }

    /// <summary>
    /// Checks if a position has any adjacent pieces within the specified distance
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row to check</param>
    /// <param name="col">Column to check</param>
    /// <param name="distance">Maximum distance to check</param>
    /// <returns>True if adjacent pieces found</returns>
    public static bool HasAdjacentPiece(GameStateModel state, int row, int col, int distance = 2) {
        for (int i = Math.Max(0, row - distance); i <= Math.Min(GameConstants.BOARD_SIZE - 1, row + distance); i++) {
            for (int j = Math.Max(0, col - distance); j <= Math.Min(GameConstants.BOARD_SIZE - 1, col + distance); j++) {
                if (state.Board[i, j] != 0) {
                    int manhattanDist = Math.Abs(i - row) + Math.Abs(j - col);
                    if (manhattanDist <= distance) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Counts adjacent pieces around a position
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row to check</param>
    /// <param name="col">Column to check</param>
    /// <returns>Number of adjacent pieces</returns>
    public static int CountAdjacentPieces(GameStateModel state, int row, int col) {
        int count = 0;
        for (int i = Math.Max(0, row - 2); i <= Math.Min(GameConstants.BOARD_SIZE - 1, row + 2); i++) {
            for (int j = Math.Max(0, col - 2); j <= Math.Min(GameConstants.BOARD_SIZE - 1, col + 2); j++) {
                if (state.Board[i, j] != 0) count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Generates a position hash for caching
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="isMaximizingPlayer">Whether it's the maximizing player's turn</param>
    /// <returns>Hash string</returns>
    public static string GeneratePositionHash(GameStateModel state, bool? isMaximizingPlayer = null) {
        var hash = new System.Text.StringBuilder();

        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                hash.Append(state.Board[i, j]);
            }
        }

        if (isMaximizingPlayer.HasValue) {
            hash.Append(isMaximizingPlayer.Value ? "A" : "H");
        }

        return hash.ToString();
    }

    /// <summary>
    /// Quick heuristic evaluation for move ordering
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="move">Move to evaluate</param>
    /// <returns>Heuristic score</returns>
    public static int QuickEvaluate(GameStateModel state, MoveModel move) {
        int centerDistance = Math.Abs(move.Row - GameConstants.BOARD_SIZE / 2) +
                             Math.Abs(move.Col - GameConstants.BOARD_SIZE / 2);
        int nearbyPieces = CountAdjacentPieces(state, move.Row, move.Col);
        return GameConstants.BOARD_SIZE - centerDistance + (nearbyPieces * 2);
    }

    /// <summary>
    /// Gets valid moves near existing pieces
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>List of nearby valid moves</returns>
    public static List<MoveModel> GetNearbyEmptySpaces(GameStateModel state) {
        List<MoveModel> moves = [];

        if (state.Board.Cast<int>().All(cell => cell == 0)) {
            return [new MoveModel {
                Row = GameConstants.BOARD_SIZE / 2,
                Col = GameConstants.BOARD_SIZE / 2
            }];
        }

        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] == 0 && HasAdjacentPiece(state, i, j, 2)) {
                    moves.Add(new MoveModel { Row = i, Col = j });
                }
            }
        }

        return [.. moves.OrderByDescending(m => QuickEvaluate(state, m))];
    }
}