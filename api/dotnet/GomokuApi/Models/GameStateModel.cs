using GomokuApi.Constants;
using GomokuApi.Utilities;

namespace GomokuApi.Models;

/// <summary>
/// Represents the current state of a Gomoku game
/// </summary>
public class GameStateModel {
    /// <summary>
    /// The game board represented as a 2D array where:
    /// 0 = Empty cell
    /// 1 = Player 1's piece
    /// 2 = Player 2's piece (AI)
    /// </summary>
    // Using Rectangular array since the board is a fixed size. If board size changed to be jagged, consider Int32[][]
    public int[,] Board { get; set; } = new int[GameConstants.BOARD_SIZE, GameConstants.BOARD_SIZE];

    /// <summary>
    /// The difficulty level of the AI
    /// Determines search depth for the minimax algorithm
    /// </summary>
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
}