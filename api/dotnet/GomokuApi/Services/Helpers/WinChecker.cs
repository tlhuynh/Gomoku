using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services.Helpers;

/// <summary>
/// Handles win condition checking and threat detection
/// </summary>
public static class WinChecker {
    /// <summary>
    /// Checks if the last move created a winning condition
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="lastMove">Last move made</param>
    /// <returns>True if win condition met</returns>
    public static bool CheckWin(GameStateModel gameState, MoveModel lastMove) {
        if (lastMove == null) return false;

        foreach (var dir in GameConstants.DIRECTIONS) {
            int count = 1;
            count += CountDirection(gameState, lastMove, dir[0], dir[1]);
            count += CountDirection(gameState, lastMove, -dir[0], -dir[1]);

            if (count >= GameConstants.WIN_LENGTH) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a position creates a winning condition
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="row">Row position</param>
    /// <param name="col">Column position</param>
    /// <returns>True if position creates win</returns>
    public static bool CheckWinAtPosition(GameStateModel state, int row, int col) {
        int player = state.Board[row, col];
        if (player == 0) return false;

        foreach (var dir in GameConstants.DIRECTIONS) {
            int count = 1;

            // Count in positive direction
            for (int i = 1; i < 5; i++) {
                int newRow = row + i * dir[0];
                int newCol = col + i * dir[1];

                if (!IsValidPosition(newRow, newCol) || state.Board[newRow, newCol] != player) {
                    break;
                }
                count++;
            }

            // Count in negative direction
            for (int i = 1; i < 5; i++) {
                int newRow = row - i * dir[0];
                int newCol = col - i * dir[1];

                if (!IsValidPosition(newRow, newCol) || state.Board[newRow, newCol] != player) {
                    break;
                }
                count++;
            }

            // Check for standard win (5 in a row)
            if (count >= 5) return true;

            // Check for double-end four (critical threat)
            if (count == 4) {
                bool positiveEndOpen = IsPositionEmpty(state, row + 4 * dir[0], col + 4 * dir[1]);
                bool negativeEndOpen = IsPositionEmpty(state, row - 1 * dir[0], col - 1 * dir[1]);

                if (positiveEndOpen && negativeEndOpen) {
                    return true; // Double-end four is effectively a win
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Detects immediate threats for both players
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>Tuple indicating AI and human winning moves</returns>
    public static (bool aiWinningMove, bool humanWinningMove) DetectImmediateThreats(GameStateModel state) {
        bool aiWinningMove = false;
        bool humanWinningMove = false;

        // Only check empty spaces adjacent to existing pieces for efficiency
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] != 0 || !BoardUtilities.HasAdjacentPiece(state, i, j, 1)) {
                    continue;
                }

                // Check AI winning move
                GameStateModel tempState = CreateTempState(state);
                tempState.Board[i, j] = (int)Player.AI;
                if (CheckWinAtPosition(tempState, i, j)) {
                    aiWinningMove = true;
                    break;
                }

                // Check human winning move
                tempState.Board[i, j] = (int)Player.Human;
                if (CheckWinAtPosition(tempState, i, j)) {
                    humanWinningMove = true;
                }
            }

            if (aiWinningMove) break;
        }

        return (aiWinningMove, humanWinningMove);
    }

    /// <summary>
    /// Counts consecutive stones in a direction
    /// </summary>
    /// <param name="gameState">Game state</param>
    /// <param name="move">Starting move</param>
    /// <param name="drow">Row direction</param>
    /// <param name="dcol">Column direction</param>
    /// <returns>Count of consecutive stones</returns>
    private static int CountDirection(GameStateModel gameState, MoveModel move, int drow, int dcol) {
        int count = 0;
        int row = move.Row + drow;
        int col = move.Col + dcol;
        int player = gameState.Board[move.Row, move.Col];

        while (IsValidPosition(row, col) && gameState.Board[row, col] == player) {
            count++;
            row += drow;
            col += dcol;
        }

        return count;
    }

    /// <summary>
    /// Checks if position is valid on board
    /// </summary>
    /// <param name="row">Row</param>
    /// <param name="col">Column</param>
    /// <returns>True if valid</returns>
    private static bool IsValidPosition(int row, int col) {
        return row >= 0 && row < GameConstants.BOARD_SIZE &&
               col >= 0 && col < GameConstants.BOARD_SIZE;
    }

    /// <summary>
    /// Checks if position is empty
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="row">Row</param>
    /// <param name="col">Column</param>
    /// <returns>True if empty and valid</returns>
    private static bool IsPositionEmpty(GameStateModel state, int row, int col) {
        return IsValidPosition(row, col) && state.Board[row, col] == 0;
    }

    /// <summary>
    /// Creates temporary state for testing
    /// </summary>
    /// <param name="state">Original state</param>
    /// <returns>Cloned state</returns>
    private static GameStateModel CreateTempState(GameStateModel state) {
        return new GameStateModel {
            Board = (int[,])state.Board.Clone(),
            Difficulty = state.Difficulty
        };
    }
}