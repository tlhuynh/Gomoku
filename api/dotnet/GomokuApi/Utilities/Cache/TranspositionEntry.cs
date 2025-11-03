using GomokuApi.Models;

namespace GomokuApi.Utilities.Cache;

/// <summary>
/// Entry in the position evaluation cache for the minimax algorithm
/// </summary>
public class TranspositionEntry {
    /// <summary>
    /// The evaluation score for this position
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// The depth at which this position was evaluated
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// The best move found for this position (optional)
    /// </summary>
    public MoveModel? BestMove { get; set; }

    /// <summary>
    /// The type of score stored (exact, lower bound, or upper bound)
    /// </summary>
    public TranspositionEntryType Type { get; set; }
}