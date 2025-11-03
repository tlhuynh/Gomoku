namespace GomokuApi.Constants;

/// <summary>
/// All constants for the Gomoku game implementation
/// </summary>
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
    /// All possible directions for line analysis (vertical, horizontal, diagonal, anti-diagonal)
    /// </summary>
    public static readonly int[][] DIRECTIONS = [
        [1, 0],   // vertical
        [0, 1],   // horizontal
        [1, 1],   // diagonal
        [1, -1]   // anti-diagonal
    ];

    /// <summary>
    /// All 8 adjacent directions for checking neighboring pieces
    /// </summary>
    public static readonly int[][] ADJACENT_DIRECTIONS = [
        [1, 0], [1, 1], [0, 1], [-1, 1],
        [-1, 0], [-1, -1], [0, -1], [1, -1]
    ];

    /// <summary>
    /// Pattern scoring values
    /// </summary>
    public static class PatternScores {
        public const int FIVE_IN_ROW = 100000;
        public const int FOUR_IN_ROW = 50000;
        public const int DOUBLE_END_FOUR = 60000;
        public const int DOUBLE_OPEN_THREE = 8000;
        public const int MULTIPLE_OPEN_THREES = 50000;
        public const int BROKEN_DOUBLE_OPEN_THREE = 1200;
        public const int THREE_WITH_EXTENSION = 800;
        public const int POTENTIAL_THREAT = 400;
        public const int CONNECTED_PAIR = 100;
        public const int SINGLE_STONE = 10;
    }

    /// <summary>
    /// Board control scoring values
    /// </summary>
    public static class BoardControlScores {
        public const int CENTER_CONTROL = 30;
        public const int MIDDLE_RING_CONTROL = 15;
        public const int STONE_CONNECTION = 5;
    }

    /// <summary>
    /// Cache and performance constants
    /// </summary>
    public static class CacheSettings {
        public const int MAX_CACHE_SIZE = 100000;
        public const int REDUCED_CACHE_SIZE = 10000;
        public const int LINE_LENGTH = 9;
        public const int LINE_CENTER = 4;
    }

    /// <summary>
    /// Search depth by difficulty
    /// </summary>
    public static class SearchDepth {
        public const int EASY = 2;
        public const int MEDIUM = 3;
        public const int HARD = 4;
        public const int EXPERT = 6;
    }
}

/// <summary>
/// Player enumeration for cleaner code
/// </summary>
public enum Player {
    None = 0,
    Human = 1,
    AI = 2
}

/// <summary>
/// Pattern types for threat detection
/// </summary>
public enum ThreatType {
    None,
    FiveInRow,
    FourInRow,
    DoubleEndFour,
    OpenThree,
    DoubleOpenThree,
    MultipleThreats
}