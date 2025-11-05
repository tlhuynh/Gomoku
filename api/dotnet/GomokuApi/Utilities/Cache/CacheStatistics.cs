namespace GomokuApi.Utilities.Cache;

/// <summary>
/// Statistics about the position cache
/// </summary>
public class CacheStatistics {
    /// <summary>
    /// Total number of entries currently stored in the cache
    /// </summary>
    public int TotalEntries { get; set; }

    /// <summary>
    /// Maximum capacity of the cache before cleanup is triggered
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// Percentage of cache capacity currently being utilized (0-100)
    /// </summary>
    public double UtilizationPercentage { get; set; }

    /// <summary>
    /// Count of entries by type (Exact, LowerBound, UpperBound)
    /// </summary>
    public Dictionary<TranspositionEntryType, int> TypeCounts { get; set; } = [];
}