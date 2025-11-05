namespace GomokuApi.Utilities;

/// <summary>
/// Represents the difficulty levels for the AI opponent
/// Each difficulty corresponds to a different search depth for the minimax algorithm
/// </summary>
public enum Difficulty {
    /// <summary>
    /// Easy difficulty - Uses a search depth of 2
    /// Suitable for beginners or casual play
    /// </summary>
    Easy = 2,

    /// <summary>
    /// Medium difficulty - Uses a search depth of 4
    /// Balanced challenge for most players
    /// </summary>
    Medium = 4,

    /// <summary>
    /// Hard difficulty - Uses a search depth of 6
    /// Challenging for experienced players
    /// </summary>
    Hard = 6,

    /// <summary>
    /// Expert difficulty - Uses a search depth of 8
    /// Very challenging, only for advanced players
    /// May have slower response times
    /// </summary>
    Expert = 8
}