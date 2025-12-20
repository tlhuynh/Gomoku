namespace GomokuApi.Models.Requests;

/// <summary>
/// Represents the game state request from the frontend
/// </summary>
public class GameStateRequestModel {
    /// <summary>
    /// The game board represented as a jagged array where:
    /// 0 = Empty cell, 1 = Player 1's piece, 2 = Player 2's piece (AI)
    /// </summary>
    public int[][]? Board { get; set; }

    /// <summary>
    /// The difficulty level as a string: "easy", "medium", "hard", or "expert"
    /// </summary>
    public string? Difficulty { get; set; }
}
