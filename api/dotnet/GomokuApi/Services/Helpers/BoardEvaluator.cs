using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services.Helpers;

/// <summary>
/// Handles board evaluation and scoring
/// </summary>
public static class BoardEvaluator {
    /// <summary>
    /// Evaluates the board position comprehensively
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>Evaluation score</returns>
    public static int EvaluateBoard(GameStateModel state) {
        // Check for immediate threats first
        var (aiWinningMove, humanWinningMove) = WinChecker.DetectImmediateThreats(state);
        if (aiWinningMove) return GameConstants.MAX_SCORE;
        if (humanWinningMove) return GameConstants.MIN_SCORE;

        int score = 0;

        // Pattern evaluation
        score += EvaluatePatterns(state);

        // Board control evaluation
        score += EvaluateBoardControl(state);

        // Stone development evaluation
        score += EvaluateStoneDevelopment(state);

        return score;
    }

    /// <summary>
    /// Evaluates patterns across all board positions
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>Pattern evaluation score</returns>
    private static int EvaluatePatterns(GameStateModel state) {
        int score = 0;

        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                score += EvaluatePosition(state, i, j);
            }
        }

        return score;
    }

    /// <summary>
    /// Evaluates a specific position by checking all lines through it
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="row">Row position</param>
    /// <param name="col">Column position</param>
    /// <returns>Position evaluation score</returns>
    private static int EvaluatePosition(GameStateModel state, int row, int col) {
        int score = 0;

        foreach (int[] dir in GameConstants.DIRECTIONS) {
            score += EvaluateLineStrength(state, row, col, dir[0], dir[1]);
        }

        return score;
    }

    /// <summary>
    /// Evaluates a line in specified direction
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="row">Starting row</param>
    /// <param name="col">Starting column</param>
    /// <param name="drow">Row direction</param>
    /// <param name="dcol">Column direction</param>
    /// <returns>Line evaluation score</returns>
    private static int EvaluateLineStrength(GameStateModel state, int row, int col, int drow, int dcol) {
        int[] line = BoardUtilities.GetLineAt(state, row, col, drow, dcol);
        int validPositions = line.Count(cell => cell != -1);

        // Need minimum positions for meaningful patterns
        if (validPositions < 5) return 0;

        int totalScore = 0;
        totalScore += PatternAnalyzer.AnalyzePatterns(line, Player.AI);
        totalScore -= PatternAnalyzer.AnalyzePatterns(line, Player.Human);

        return totalScore;
    }

    /// <summary>
    /// Evaluates board control (center and strategic positions)
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>Board control score</returns>
    private static int EvaluateBoardControl(GameStateModel state) {
        int score = 0;
        int center = GameConstants.BOARD_SIZE / 2;

        // Evaluate center area control (3x3)
        for (int i = center - 1; i <= center + 1; i++) {
            for (int j = center - 1; j <= center + 1; j++) {
                if (IsValidPosition(i, j)) {
                    if (state.Board[i, j] == (int)Player.AI) {
                        score += GameConstants.BoardControlScores.CENTER_CONTROL;
                    } else if (state.Board[i, j] == (int)Player.Human) {
                        score -= GameConstants.BoardControlScores.CENTER_CONTROL;
                    }
                }
            }
        }

        // Evaluate middle ring control (5x5 excluding center)
        for (int i = center - 2; i <= center + 2; i++) {
            for (int j = center - 2; j <= center + 2; j++) {
                if (IsValidPosition(i, j) && (Math.Abs(i - center) > 1 || Math.Abs(j - center) > 1)) {
                    if (state.Board[i, j] == (int)Player.AI) {
                        score += GameConstants.BoardControlScores.MIDDLE_RING_CONTROL;
                    } else if (state.Board[i, j] == (int)Player.Human) {
                        score -= GameConstants.BoardControlScores.MIDDLE_RING_CONTROL;
                    }
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Evaluates stone development and connections
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>Stone development score</returns>
    private static int EvaluateStoneDevelopment(GameStateModel state) {
        int score = 0;

        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] == 0) continue;

                int player = state.Board[i, j];
                int connectionScore = player == (int)Player.AI ?
                    GameConstants.BoardControlScores.STONE_CONNECTION :
                    -GameConstants.BoardControlScores.STONE_CONNECTION;

                // Check all adjacent directions for connections
                foreach (var dir in GameConstants.ADJACENT_DIRECTIONS) {
                    int ni = i + dir[0];
                    int nj = j + dir[1];

                    if (IsValidPosition(ni, nj) && state.Board[ni, nj] == player) {
                        score += connectionScore;
                    }
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Detects critical threats with scoring
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="isMaximizing">Whether maximizing player's turn</param>
    /// <returns>Critical threat score</returns>
    public static int DetectCriticalThreats(GameStateModel state, bool isMaximizing) {
        ThreatInfo threats = AnalyzeAllThreats(state);

        // Return scores based on threats found and whose turn it is
        if (threats.HumanDoubleEndFour) {
            return isMaximizing ? GameConstants.MIN_SCORE + 150 : GameConstants.MIN_SCORE + 50;
        }

        if (threats.AiDoubleEndFour) {
            return isMaximizing ? GameConstants.MAX_SCORE - 50 : GameConstants.MAX_SCORE - 150;
        }

        if (threats.HumanOpenThrees >= 2) {
            return isMaximizing ? GameConstants.MIN_SCORE + 200 : GameConstants.MIN_SCORE + 200;
        }

        if (threats.AiOpenThrees >= 2) {
            return isMaximizing ? GameConstants.MAX_SCORE - 200 : GameConstants.MAX_SCORE - 200;
        }

        if (threats.HumanOpenThrees >= 1) {
            return isMaximizing ? GameConstants.MIN_SCORE + 500 : GameConstants.MIN_SCORE + 500;
        }

        if (threats.AiOpenThrees >= 1) {
            return isMaximizing ? GameConstants.MAX_SCORE - 500 : GameConstants.MAX_SCORE - 500;
        }

        return 0; // No critical threats
    }

    /// <summary>
    /// Analyzes all threats on the board
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>Threat analysis</returns>
    private static ThreatInfo AnalyzeAllThreats(GameStateModel state) {
        ThreatInfo threats = new();

        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] != 0) continue;

                foreach (int[] dir in GameConstants.DIRECTIONS) {
                    int[] line = BoardUtilities.GetLineAt(state, i, j, dir[0], dir[1]);
                    AnalyzeThreatPatterns(line, ref threats);
                }
            }
        }

        return threats;
    }

    /// <summary>
    /// Analyzes threat patterns in a line
    /// </summary>
    /// <param name="line">Line to analyze</param>
    /// <param name="threats">Threat info to update</param>
    private static void AnalyzeThreatPatterns(int[] line, ref ThreatInfo threats) {
        // Check double-end four patterns
        if (PatternAnalyzer.ContainsPattern(line, (int)Player.AI, [0, (int)Player.AI, (int)Player.AI, (int)Player.AI, (int)Player.AI, 0])) {
            threats.AiDoubleEndFour = true;
        }

        if (PatternAnalyzer.ContainsPattern(line, (int)Player.Human, [0, (int)Player.Human, (int)Player.Human, (int)Player.Human, (int)Player.Human, 0])) {
            threats.HumanDoubleEndFour = true;
        }

        // Check open three patterns
        if (PatternAnalyzer.ContainsPattern(line, (int)Player.AI, [0, 0, (int)Player.AI, (int)Player.AI, (int)Player.AI, 0, 0])) {
            threats.AiOpenThrees++;
        }

        if (PatternAnalyzer.ContainsPattern(line, (int)Player.Human, [0, 0, (int)Player.Human, (int)Player.Human, (int)Player.Human, 0, 0])) {
            threats.HumanOpenThrees++;
        }
    }

    /// <summary>
    /// Checks if position is valid
    /// </summary>
    /// <param name="row">Row</param>
    /// <param name="col">Column</param>
    /// <returns>True if valid</returns>
    private static bool IsValidPosition(int row, int col) {
        return row >= 0 && row < GameConstants.BOARD_SIZE &&
               col >= 0 && col < GameConstants.BOARD_SIZE;
    }

    /// <summary>
    /// Information about threats on the board
    /// </summary>
    private struct ThreatInfo {
        public bool AiDoubleEndFour;
        public bool HumanDoubleEndFour;
        public int AiOpenThrees;
        public int HumanOpenThrees;
    }
}