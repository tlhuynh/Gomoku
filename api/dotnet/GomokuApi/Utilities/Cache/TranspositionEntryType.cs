namespace GomokuApi.Utilities.Cache;

/// <summary>
/// Type of score stored in the transposition table
/// </summary>
public enum TranspositionEntryType {
    /// <summary>
    /// Exact evaluation score
    /// </summary>
    Exact,
    
    /// <summary>
    /// A lower bound on the score
    /// </summary>
    LowerBound,
    
    /// <summary>
    /// An upper bound on the score
    /// </summary>
    UpperBound
}