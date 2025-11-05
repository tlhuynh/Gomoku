namespace GomokuApi.Models;

/// <summary>
/// Represents a move made by either player in the game
/// </summary>
public class MoveModel {
    /// <summary>
    /// The row index of the move
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// The column index of the move
    /// </summary>
    public int Col { get; set; }
}