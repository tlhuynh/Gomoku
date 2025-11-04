using GomokuApi.Constants;
using GomokuApi.Models;
using GomokuApi.Services.Helpers;
using GomokuApi.Utilities.Cache;

namespace GomokuApi.Services;

/// <summary>
/// Service for managing position evaluation cache in the minimax algorithm
/// </summary>
public class PositionCacheService : IPositionCacheService {
    private readonly Dictionary<string, TranspositionEntry> _positionCache = new(GameConstants.CacheSettings.MAX_CACHE_SIZE);

    /// <summary>
    /// Attempts to retrieve a cached position evaluation
    /// </summary>
    /// <param name="positionHash">Hash of the board position</param>
    /// <param name="depth">Minimum required depth</param>
    /// <param name="alpha">Alpha value for bound checking</param>
    /// <param name="beta">Beta value for bound checking</param>
    /// <returns>Cached score if valid, null otherwise</returns>
    public int? GetCachedScore(string positionHash, int depth, int alpha, int beta) {
        if (_positionCache.TryGetValue(positionHash, out TranspositionEntry? cachedEntry) && cachedEntry.Depth >= depth) {
            switch (cachedEntry.Type) {
                case TranspositionEntryType.Exact:
                    return cachedEntry.Score;
                case TranspositionEntryType.LowerBound when cachedEntry.Score >= beta:
                    return cachedEntry.Score;
                case TranspositionEntryType.UpperBound when cachedEntry.Score <= alpha:
                    return cachedEntry.Score;
            }
        }
        return null;
    }

    /// <summary>
    /// Caches a position evaluation result
    /// </summary>
    /// <param name="positionHash">Hash of the board position</param>
    /// <param name="score">Evaluation score</param>
    /// <param name="depth">Search depth used</param>
    /// <param name="originalAlpha">Original alpha value</param>
    /// <param name="beta">Beta value</param>
    public void CachePosition(string positionHash, int score, int depth, int originalAlpha, int beta) {
        TranspositionEntryType entryType;
        if (score <= originalAlpha) {
            entryType = TranspositionEntryType.UpperBound;
        } else if (score >= beta) {
            entryType = TranspositionEntryType.LowerBound;
        } else {
            entryType = TranspositionEntryType.Exact;
        }

        _positionCache[positionHash] = new TranspositionEntry {
            Score = score,
            Depth = depth,
            Type = entryType
        };
    }

    /// <summary>
    /// Cleans cache when it gets too large. Keeps the most valuable entries based on depth.
    /// </summary>
    public void CleanCacheIfNeeded() {
        if (_positionCache.Count <= GameConstants.CacheSettings.MAX_CACHE_SIZE) {
            return;
        }

        Dictionary<string, TranspositionEntry> valuableEntries = _positionCache
            .OrderByDescending(entry => entry.Value.Depth)
            .Take(GameConstants.CacheSettings.REDUCED_CACHE_SIZE)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        _positionCache.Clear();
        foreach (KeyValuePair<string, TranspositionEntry> entry in valuableEntries) {
            _positionCache[entry.Key] = entry.Value;
        }
    }

    /// <summary>
    /// Generates a position hash for caching
    /// </summary>
    /// <param name="state">Game state to hash</param>
    /// <param name="isMaximizing">Whether this is a maximizing player's turn</param>
    /// <returns>Unique hash string for the position</returns>
    public string GeneratePositionHash(GameStateModel state, bool isMaximizing) {
        return BoardUtilities.GeneratePositionHash(state, isMaximizing);
    }

    /// <summary>
    /// Gets cache statistics for monitoring
    /// </summary>
    /// <returns>Cache statistics</returns>
    public CacheStatistics GetCacheStatistics() {
        Dictionary<TranspositionEntryType, int> typeCounts = [];
        foreach (TranspositionEntry entry in _positionCache.Values) {
            if (typeCounts.ContainsKey(entry.Type)) {
                typeCounts[entry.Type]++;
            } else {
                typeCounts[entry.Type] = 1;
            }
        }

        return new CacheStatistics {
            TotalEntries = _positionCache.Count,
            MaxCapacity = GameConstants.CacheSettings.MAX_CACHE_SIZE,
            UtilizationPercentage = (double)_positionCache.Count / GameConstants.CacheSettings.MAX_CACHE_SIZE * 100,
            TypeCounts = typeCounts
        };
    }

    /// <summary>
    /// Clears the entire cache
    /// </summary>
    public void ClearCache() {
        _positionCache.Clear();
    }
}