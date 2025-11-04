using GomokuApi.Models;
using GomokuApi.Utilities.Cache;

namespace GomokuApi.Services;

/// <summary>
/// Interface for position evaluation cache management in the minimax algorithm
/// </summary>
public interface IPositionCacheService {
    /// <summary>
    /// Attempts to retrieve a cached position evaluation
    /// </summary>
    /// <param name="positionHash">Hash of the board position</param>
    /// <param name="depth">Minimum required depth</param>
    /// <param name="alpha">Alpha value for bound checking</param>
    /// <param name="beta">Beta value for bound checking</param>
    /// <returns>Cached score if valid, null otherwise</returns>
    int? GetCachedScore(string positionHash, int depth, int alpha, int beta);

    /// <summary>
    /// Caches a position evaluation result
    /// </summary>
    /// <param name="positionHash">Hash of the board position</param>
    /// <param name="score">Evaluation score</param>
    /// <param name="depth">Search depth used</param>
    /// <param name="originalAlpha">Original alpha value</param>
    /// <param name="beta">Beta value</param>
    void CachePosition(string positionHash, int score, int depth, int originalAlpha, int beta);

    /// <summary>
    /// Cleans cache when it gets too large
    /// </summary>
    void CleanCacheIfNeeded();

    /// <summary>
    /// Generates a position hash for caching
    /// </summary>
    /// <param name="state">Game state to hash</param>
    /// <param name="isMaximizing">Whether this is a maximizing player's turn</param>
    /// <returns>Unique hash string for the position</returns>
    string GeneratePositionHash(GameStateModel state, bool isMaximizing);

    /// <summary>
    /// Gets cache statistics for monitoring
    /// </summary>
    /// <returns>Cache statistics</returns>
    CacheStatistics GetCacheStatistics();

    /// <summary>
    /// Clears the entire cache
    /// </summary>
    void ClearCache();
}