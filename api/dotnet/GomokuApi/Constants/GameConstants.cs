namespace GomokuApi.Constants;

public static class GameConstants {
    /// <summary>
    /// Size of the Gomoku board (default to: 15x15)
    /// </summary>
    public const int BOARD_SIZE = 15;

    /// <summary>
    /// Number of pieces in a row needed to win
    /// </summary>
    public const int WIN_LENGTH = 5;

    /// <summary>
    /// Maximum possible evaluation score for a winning position
    /// </summary>
    public const int MAX_SCORE = 1000000;

    /// <summary>
    /// Minimum possible evaluation score for a losing position
    /// </summary>
    public const int MIN_SCORE = -1000000;

    /// <summary>
    /// Default time limit for AI move calculation in milliseconds
    /// </summary>
    public const int DEFAULT_TIME_LIMIT_MS = 5000; // 5 seconds default
}