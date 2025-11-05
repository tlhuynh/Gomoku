using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services.Helpers;

/// <summary>
/// Utility methods for board operations and analysis
/// </summary>
public static class BoardUtilities {
    /// <summary>
    /// Gets a line of cells in a specific direction from a given position
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Starting row</param>
    /// <param name="col">Starting column</param>
    /// <param name="drow">Row direction</param>
    /// <param name="dcol">Column direction</param>
    /// <returns>Array of cell values in the line</returns>
    public static int[] GetLineAt(GameStateModel state, int row, int col, int drow, int dcol) {
        int[] line = new int[GameConstants.CacheSettings.LINE_LENGTH];
        int center = GameConstants.CacheSettings.LINE_CENTER;

        for (int i = 0; i < GameConstants.CacheSettings.LINE_LENGTH; i++) {
            int newRow = row + (i - center) * drow;
            int newCol = col + (i - center) * dcol;

            int value = -1; // Out of bounds marker
            if (newRow >= 0 && newRow < GameConstants.BOARD_SIZE &&
                newCol >= 0 && newCol < GameConstants.BOARD_SIZE) {
                value = state.Board[newRow, newCol];
            }

            line[i + GameConstants.CacheSettings.LINE_CENTER] = value;
        }

        return line;
    }

    /// <summary>
    /// Checks if a position has any adjacent pieces within the specified distance
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row to check</param>
    /// <param name="col">Column to check</param>
    /// <param name="distance">Maximum distance to check</param>
    /// <returns>True if adjacent pieces found</returns>
    public static bool HasAdjacentPiece(GameStateModel state, int row, int col, int distance = 2) {
        for (int i = Math.Max(0, row - distance); i <= Math.Min(GameConstants.BOARD_SIZE - 1, row + distance); i++) {
            for (int j = Math.Max(0, col - distance); j <= Math.Min(GameConstants.BOARD_SIZE - 1, col + distance); j++) {
                if (state.Board[i, j] != 0) {
                    int manhattanDist = Math.Abs(i - row) + Math.Abs(j - col);
                    if (manhattanDist <= distance) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Counts adjacent pieces around a position
    /// </summary>
    /// <param name="state">Current game state</param>
    /// <param name="row">Row to check</param>
    /// <param name="col">Column to check</param>
    /// <returns>Number of adjacent pieces</returns>
    public static int CountAdjacentPieces(GameStateModel state, int row, int col) {
        int count = 0;
        for (int i = Math.Max(0, row - 2); i <= Math.Min(GameConstants.BOARD_SIZE - 1, row + 2); i++) {
            for (int j = Math.Max(0, col - 2); j <= Math.Min(GameConstants.BOARD_SIZE - 1, col + 2); j++) {
                if (state.Board[i, j] != 0) count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Generates a position hash for caching
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="isMaximizingPlayer">Whether it's the maximizing player's turn</param>
    /// <returns>Hash string</returns>
    public static string GeneratePositionHash(GameStateModel state, bool? isMaximizingPlayer = null) {
        System.Text.StringBuilder hash = new();

        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                hash.Append(state.Board[i, j]);
            }
        }

        if (isMaximizingPlayer.HasValue) {
            hash.Append(isMaximizingPlayer.Value ? "A" : "H");
        }

        return hash.ToString();
    }

    /// <summary>
    /// Enhanced quick heuristic evaluation for move ordering with caching
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="move">Move to evaluate</param>
    /// <returns>Heuristic score</returns>
    public static int QuickEvaluate(GameStateModel state, MoveModel move) {
        // Center preference (closer to center is better)
        int centerDistance = Math.Abs(move.Row - GameConstants.BOARD_SIZE / 2) +
                             Math.Abs(move.Col - GameConstants.BOARD_SIZE / 2);
        int centerScore = GameConstants.BOARD_SIZE - centerDistance;

        // Nearby pieces bonus (more activity around the position)
        int nearbyPieces = CountAdjacentPieces(state, move.Row, move.Col);
        int proximityScore = nearbyPieces * 3;

        // Strategic position evaluation
        int strategicScore = EvaluateStrategicPosition(state, move);

        return centerScore + proximityScore + strategicScore;
    }

    /// <summary>
    /// Evaluates strategic value of a position based on potential patterns
    /// </summary>
    private static int EvaluateStrategicPosition(GameStateModel state, MoveModel move) {
        int score = 0;

        // Check all directions for potential patterns
        foreach (int[] direction in GameConstants.DIRECTIONS) {
            int[] line = GetLineAt(state, move.Row, move.Col, direction[0], direction[1]);

            // Look for potential to form or block patterns
            score += EvaluateLineForPosition(line);
        }

        return score;
    }

    /// <summary>
    /// Evaluates a line for strategic positioning value
    /// </summary>
    private static int EvaluateLineForPosition(int[] line) {
        int score = 0;
        int center = line.Length / 2;

        // Check immediate neighbors for pattern potential
        for (int i = Math.Max(0, center - 2); i <= Math.Min(line.Length - 1, center + 2); i++) {
            if (i != center && line[i] != 0 && line[i] != -1) {
                score += 2; // Bonus for being near existing pieces
            }
        }

        return score;
    }

    /// <summary>
    /// Performance-aware version that chooses the best algorithm based on game state
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="forceSequential">Force sequential processing for testing</param>
    /// <returns>Performance statistics and moves</returns>
    public static (List<MoveModel> moves, long executionTimeMs, string algorithmUsed) GetNearbyEmptySpacesWithStats(
        GameStateModel state, bool forceSequential = false) {

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        List<MoveModel> moves;
        string algorithm;

        if (forceSequential) {
            moves = GetNearbyEmptySpacesOptimized(state);
            algorithm = "Sequential-Optimized";
        } else {
            int totalPieces = state.Board.Cast<int>().Count(cell => cell != 0);
            if (totalPieces <= 6 || GameConstants.BOARD_SIZE <= 10) {
                moves = GetNearbyEmptySpacesOptimized(state);
                algorithm = "Sequential-Optimized";
            } else {
                moves = GetNearbyEmptySpacesParallel(state);
                algorithm = "Parallel";
            }
        }

        stopwatch.Stop();
        return (moves, stopwatch.ElapsedMilliseconds, algorithm);
    }

    /// <summary>
    /// Gets valid moves near existing pieces with optimized parallel processing
    /// </summary>
    /// <param name="state">Game state</param>
    /// <returns>List of nearby valid moves</returns>
    public static List<MoveModel> GetNearbyEmptySpaces(GameStateModel state) {
        // Handle empty board case
        if (state.Board.Cast<int>().All(cell => cell == 0)) {
            return [new MoveModel {
                Row = GameConstants.BOARD_SIZE / 2,
                Col = GameConstants.BOARD_SIZE / 2
            }];
        }

        // For small boards or few pieces, use optimized sequential approach
        int totalPieces = state.Board.Cast<int>().Count(cell => cell != 0);
        if (totalPieces <= 6 || GameConstants.BOARD_SIZE <= 10) {
            return GetNearbyEmptySpacesOptimized(state);
        }

        // Use parallel processing for larger, more complex positions
        return GetNearbyEmptySpacesParallel(state);
    }

    /// <summary>
    /// Optimized sequential approach for smaller game states
    /// </summary>
    private static List<MoveModel> GetNearbyEmptySpacesOptimized(GameStateModel state) {
        HashSet<(int row, int col)> candidatePositions = [];

        // First pass: Find all occupied positions and their neighbors
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] != 0) {
                    // Add all empty positions within distance 4
                    AddNearbyEmptyPositions(state, i, j, candidatePositions);
                }
            }
        }

        // Convert to MoveModel list and evaluate
        List<MoveModel> moves = [.. candidatePositions.Select(pos => new MoveModel { Row = pos.row, Col = pos.col })];

        return [.. moves.OrderByDescending(m => QuickEvaluate(state, m))];
    }

    /// <summary>
    /// Parallel processing approach for larger game states
    /// </summary>
    private static List<MoveModel> GetNearbyEmptySpacesParallel(GameStateModel state) {
        object lockObject = new();
        HashSet<(int row, int col)> candidatePositions = [];

        // Find all occupied positions
        List<(int row, int col)> occupiedPositions = [];
        for (int i = 0; i < GameConstants.BOARD_SIZE; i++) {
            for (int j = 0; j < GameConstants.BOARD_SIZE; j++) {
                if (state.Board[i, j] != 0) {
                    occupiedPositions.Add((i, j));
                }
            }
        }

        // Parallel processing of occupied positions
        ParallelOptions parallelOptions = new() {
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, occupiedPositions.Count)
        };

        Parallel.ForEach(occupiedPositions, parallelOptions, pos => {
            HashSet<(int row, int col)> localCandidates = [];
            AddNearbyEmptyPositions(state, pos.row, pos.col, localCandidates);

            lock (lockObject) {
                foreach (var candidate in localCandidates) {
                    candidatePositions.Add(candidate);
                }
            }
        });

        // Parallel evaluation and sorting
        List<MoveModel> moves = [.. candidatePositions.Select(pos => new MoveModel { Row = pos.row, Col = pos.col })];

        // Parallel evaluation of moves
        double center = GameConstants.BOARD_SIZE / 2.0;

        // Convert to evaluated moves and sort by proximity to center
        List<MoveModel> evaluatedMoves = moves.AsParallel()
            .Select(move => (
                move,
                distance: Math.Sqrt(Math.Pow(move.Row - center, 2) + Math.Pow(move.Col - center, 2))
            ))
            .OrderBy(x => x.distance)
            .Select(x => x.move)
            .ToList();

        return evaluatedMoves;
    }

    /// <summary>
    /// Helper method to add nearby empty positions around an occupied cell
    /// </summary>
    private static void AddNearbyEmptyPositions(GameStateModel state, int centerRow, int centerCol,
        HashSet<(int row, int col)> positions) {
        const int distance = 4;

        for (int i = Math.Max(0, centerRow - distance);
             i <= Math.Min(GameConstants.BOARD_SIZE - 1, centerRow + distance); i++) {
            for (int j = Math.Max(0, centerCol - distance);
                 j <= Math.Min(GameConstants.BOARD_SIZE - 1, centerCol + distance); j++) {

                if (state.Board[i, j] == 0) {
                    int manhattanDist = Math.Abs(i - centerRow) + Math.Abs(j - centerCol);
                    if (manhattanDist <= distance) {
                        positions.Add((i, j));
                    }
                }
            }
        }
    }
}