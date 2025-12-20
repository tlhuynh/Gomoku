namespace GomokuApi.Models.Responses;

/// <summary>
/// Represents the AI's response to a move request, including the chosen move and its evaluation score
/// </summary>
public class MoveResponseModel {
    /// <summary>
    /// The move chosen by the AI
    /// </summary>
    public required MoveModel Move { get; set; }

    /// <summary>
    /// The evaluation score for the move:
    /// Positive values indicate advantage for AI
    /// Negative values indicate advantage for human player
    /// Larger absolute values indicate stronger positions
    /// </summary>
    public int Score { get; set; }
}
