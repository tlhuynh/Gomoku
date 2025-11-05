using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services.Helpers;

/// <summary>
/// Analyzes patterns and threats on the game board
/// </summary>
public static class PatternAnalyzer {
    /// <summary>
    /// Result of threat analysis
    /// </summary>
    public record ThreatAnalysis(bool HasThreat, ThreatType Type, int Score);

    /// <summary>
    /// Analyzes patterns in a line for a specific player
    /// </summary>
    /// <param name="line">Line to analyze</param>
    /// <param name="player">Player to analyze for</param>
    /// <returns>Pattern analysis score</returns>
    public static int AnalyzePatterns(int[] line, Player player) {
        int playerValue = (int)player;
        int score = 0;

        // Check for winning pattern (five or more in a row)
        if (ContainsConsecutive(line, playerValue, 5)) {
            return player == Player.AI ? GameConstants.MAX_SCORE : GameConstants.MIN_SCORE;
        }

        // Check for four in a row (immediate winning threat)
        if (ContainsConsecutive(line, playerValue, 4)) {
            return player == Player.AI ? GameConstants.PatternScores.FOUR_IN_ROW : -GameConstants.PatternScores.FOUR_IN_ROW;
        }

        // Check for double-end four
        if (ContainsPattern(line, playerValue, [0, playerValue, playerValue, playerValue, playerValue, 0])) {
            return player == Player.AI ? GameConstants.PatternScores.DOUBLE_END_FOUR : -GameConstants.PatternScores.DOUBLE_END_FOUR;
        }

        // Check for double-open three
        if (ContainsPattern(line, playerValue, [0, 0, playerValue, playerValue, playerValue, 0, 0])) {
            return player == Player.AI ? GameConstants.PatternScores.DOUBLE_OPEN_THREE : -GameConstants.PatternScores.DOUBLE_OPEN_THREE;
        }

        // Check for multiple open threes
        int openThrees = CountPattern(line, playerValue, [0, playerValue, playerValue, playerValue, 0]);
        if (openThrees >= 2) {
            return player == Player.AI ? GameConstants.PatternScores.MULTIPLE_OPEN_THREES : -GameConstants.PatternScores.MULTIPLE_OPEN_THREES;
        } else if (openThrees == 1) {
            return player == Player.AI ? GameConstants.PatternScores.DOUBLE_OPEN_THREE : -GameConstants.PatternScores.DOUBLE_OPEN_THREE;
        }

        // Add scores for other patterns
        score += AnalyzeSecondaryPatterns(line, playerValue, player);

        return score;
    }

    /// <summary>
    /// Analyzes secondary patterns (three in a row, pairs, etc.)
    /// </summary>
    private static int AnalyzeSecondaryPatterns(int[] line, int playerValue, Player player) {
        int score = 0;
        int multiplier = player == Player.AI ? 1 : -1;

        // Various three-stone patterns
        int[][] threePatterns = [
            [playerValue, playerValue, playerValue, 0, 0],
            [0, 0, playerValue, playerValue, playerValue],
            [playerValue, playerValue, 0, playerValue, 0],
            [0, playerValue, 0, playerValue, playerValue],
            [playerValue, 0, playerValue, playerValue, 0]
        ];

        foreach (int[] pattern in threePatterns) {
            if (ContainsPattern(line, playerValue, pattern)) {
                score += GameConstants.PatternScores.THREE_WITH_EXTENSION * multiplier;
            }
        }

        // Broken double-open threes
        int[][] brokenPatterns = [
            [0, 0, playerValue, 0, playerValue, playerValue, 0, 0],
            [0, 0, playerValue, playerValue, 0, playerValue, 0, 0]
        ];

        foreach (int[] pattern in brokenPatterns) {
            if (ContainsPattern(line, playerValue, pattern)) {
                score += GameConstants.PatternScores.BROKEN_DOUBLE_OPEN_THREE * multiplier;
            }
        }

        // Potential threats
        int[][] threatPatterns = [
            [0, playerValue, playerValue, 0, 0],
            [0, 0, playerValue, playerValue, 0],
            [0, playerValue, 0, playerValue, 0]
        ];

        foreach (int[] pattern in threatPatterns) {
            if (ContainsPattern(line, playerValue, pattern)) {
                score += GameConstants.PatternScores.POTENTIAL_THREAT * multiplier;
            }
        }

        // Connected pairs
        if (ContainsPattern(line, playerValue, [0, 0, playerValue, playerValue, 0, 0])) {
            score += GameConstants.PatternScores.CONNECTED_PAIR * multiplier;
        }

        // Single stones
        int singleStones = line.Count(cell => cell == playerValue);
        score += singleStones * GameConstants.PatternScores.SINGLE_STONE * multiplier;

        return score;
    }

    /// <summary>
    /// Checks if a move creates a guaranteed winning position
    /// </summary>
    /// <param name="state">Game state after move</param>
    /// <param name="move">Move made</param>
    /// <param name="player">Player who made the move</param>
    /// <returns>True if guaranteed win</returns>
    public static bool CheckGuaranteedWin(GameStateModel state, MoveModel move, Player player) {
        int playerValue = (int)player;

        foreach (int[] dir in GameConstants.DIRECTIONS) {
            int[] line = BoardUtilities.GetLineAt(state, move.Row, move.Col, dir[0], dir[1]);

            // Double-end four pattern
            if (ContainsPattern(line, playerValue, [0, playerValue, playerValue, playerValue, playerValue, 0])) {
                return true;
            }

            // Open three with multiple threats
            if (ContainsPattern(line, playerValue, [0, 0, playerValue, playerValue, playerValue, 0, 0])) {
                int threatsCount = CountThreatsAfterMove(state, player);
                if (threatsCount >= 2) {
                    return true;
                }
            }

            // Disconnected guaranteed win patterns
            if (HasDisconnectedGuaranteedWinPattern(line, playerValue)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for disconnected patterns that create guaranteed wins
    /// </summary>
    /// <param name="state">Game state after move</param>
    /// <param name="move">Move made</param>
    /// <param name="player">Player who made the move</param>
    /// <returns>True if disconnected pattern creates guaranteed win</returns>
    public static bool CheckDisconnectedGuaranteedWin(GameStateModel state, MoveModel move, Player player) {
        int playerValue = (int)player;

        foreach (int[] dir in GameConstants.DIRECTIONS) {
            int[] line = BoardUtilities.GetLineAt(state, move.Row, move.Col, dir[0], dir[1]);

            if (HasDisconnectedGuaranteedWinPattern(line, playerValue)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for open three formations
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="move">Move made</param>
    /// <param name="player">Player</param>
    /// <returns>True if strong open three formed</returns>
    public static bool CheckOpenThreeFormation(GameStateModel state, MoveModel move, Player player) {
        int playerValue = (int)player;

        foreach (int[] dir in GameConstants.DIRECTIONS) {
            int[] line = BoardUtilities.GetLineAt(state, move.Row, move.Col, dir[0], dir[1]);

            if (ContainsPattern(line, playerValue, [0, 0, playerValue, playerValue, playerValue, 0, 0])) {
                return true;
            }

            if (ContainsPattern(line, playerValue, [0, playerValue, playerValue, playerValue, 0])) {
                int openThreeCount = GameConstants.DIRECTIONS
                    .Select(checkDir => BoardUtilities.GetLineAt(state, move.Row, move.Col, checkDir[0], checkDir[1]))
                    .Count(checkLine => ContainsPattern(checkLine, playerValue, [0, playerValue, playerValue, playerValue, 0]));

                if (openThreeCount >= 2) {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Counts potential winning threats
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="player">Player</param>
    /// <returns>Number of threats</returns>
    public static int CountThreatsAfterMove(GameStateModel state, Player player) {
        int threats = 0;
        int playerValue = (int)player;

        for (int i = 0; i < GameConstants.BOARD_SIZE && threats < 2; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE && threats < 2; j++) {
                if (state.Board[i, j] != 0) continue;

                GameStateModel tempState = new() {
                    Board = (int[,])state.Board.Clone(),
                    Difficulty = state.Difficulty
                };
                tempState.Board[i, j] = playerValue;

                if (CheckFourInRowThreat(tempState, i, j, player)) {
                    threats++;
                }
            }
        }

        return threats;
    }

    /// <summary>
    /// Checks for four-in-a-row threat
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="row">Row</param>
    /// <param name="col">Column</param>
    /// <param name="player">Player</param>
    /// <returns>True if four-in-a-row threat exists</returns>
    public static bool CheckFourInRowThreat(GameStateModel state, int row, int col, Player player) {
        int playerValue = (int)player;

        foreach (int[] dir in GameConstants.DIRECTIONS) {
            int count = 1;

            // Count in both directions
            for (int direction = -1; direction <= 1; direction += 2) {
                for (int i = 1; i < 4; i++) {
                    int newRow = row + i * dir[0] * direction;
                    int newCol = col + i * dir[1] * direction;

                    if (newRow < 0 || newRow >= GameConstants.BOARD_SIZE ||
                        newCol < 0 || newCol >= GameConstants.BOARD_SIZE ||
                        state.Board[newRow, newCol] != playerValue) {
                        break;
                    }
                    count++;
                }
            }

            if (count >= 4) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if line contains specific pattern
    /// </summary>
    /// <param name="line">Line to check</param>
    /// <param name="player">Player value</param>
    /// <param name="pattern">Pattern to match</param>
    /// <returns>True if pattern found</returns>
    public static bool ContainsPattern(int[] line, int player, int[] pattern) {
        for (int i = 0; i <= line.Length - pattern.Length; i++) {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++) {
                if (line[i + j] == -1) {
                    match = false;
                    break;
                }

                if (pattern[j] == player && line[i + j] != player) {
                    match = false;
                    break;
                } else if (pattern[j] != 0 && pattern[j] != player && line[i + j] != pattern[j]) {
                    match = false;
                    break;
                } else if (pattern[j] == 0 && line[i + j] != 0) {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }
        return false;
    }

    /// <summary>
    /// Counts pattern occurrences
    /// </summary>
    /// <param name="line">Line to check</param>
    /// <param name="player">Player value</param>
    /// <param name="pattern">Pattern to count</param>
    /// <returns>Number of occurrences</returns>
    public static int CountPattern(int[] line, int player, int[] pattern) {
        int count = 0;
        for (int i = 0; i <= line.Length - pattern.Length; i++) {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++) {
                if (line[i + j] == -1) {
                    match = false;
                    break;
                }
                if (pattern[j] != 0 && line[i + j] != pattern[j]) {
                    match = false;
                    break;
                }
            }
            if (match) count++;
        }
        return count;
    }

    /// <summary>
    /// Checks for consecutive stones
    /// </summary>
    /// <param name="line">Line to check</param>
    /// <param name="player">Player value</param>
    /// <param name="n">Number of consecutive stones</param>
    /// <returns>True if found</returns>
    public static bool ContainsConsecutive(int[] line, int player, int n) {
        int consecutive = 0;
        for (int i = 0; i < line.Length; i++) {
            if (line[i] == player) {
                consecutive++;
                if (consecutive >= n) return true;
            } else {
                consecutive = 0;
            }
        }
        return false;
    }

    /// <summary>
    /// Detects specific disconnected patterns that guarantee wins
    /// </summary>
    /// <param name="line">Line to analyze</param>
    /// <param name="player">Player value</param>
    /// <returns>True if guaranteed win pattern found</returns>
    private static bool HasDisconnectedGuaranteedWinPattern(int[] line, int player) {
        // Pattern 1: _XX_X_ - Creates 3 simultaneous threats
        if (ContainsPattern(line, player, [0, player, player, 0, player, 0])) {
            return true;
        }

        // Pattern 2: _X_XX_ - Creates 3 simultaneous threats  
        if (ContainsPattern(line, player, [0, player, 0, player, player, 0])) {
            return true;
        }

        // Pattern 3: _XXX_X_ - Extended disconnected pattern
        if (ContainsPattern(line, player, [0, player, player, player, 0, player, 0])) {
            return true;
        }

        // Pattern 4: _X_XXX_ - Extended disconnected pattern
        if (ContainsPattern(line, player, [0, player, 0, player, player, player, 0])) {
            return true;
        }

        // Pattern 5: _XX_XX_ - Double disconnected pair
        if (ContainsPattern(line, player, [0, player, player, 0, player, player, 0])) {
            return true;
        }

        // Pattern 6: _X_X_X_ - Spaced pattern creating multiple threats
        if (ContainsPattern(line, player, [0, player, 0, player, 0, player, 0])) {
            return true;
        }

        return false;
    }
}