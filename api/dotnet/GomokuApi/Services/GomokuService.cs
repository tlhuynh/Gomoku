using GomokuApi.Constants;
using GomokuApi.Models;
using GomokuApi.Services.Helpers;
using GomokuApi.Utilities.Cache;

namespace GomokuApi.Services;

/// <summary>
/// Implementation of the Gomoku game service providing AI gameplay and game mechanics
/// </summary>
public class GomokuService : IGomokuService {
    /// <summary>
    /// Cache for position evaluations to improve performance
    /// </summary>
    private readonly Dictionary<string, TranspositionEntry> _positionCache = new(GameConstants.CacheSettings.MAX_CACHE_SIZE);

    /// <summary>
    /// Determines the best move for the AI player using minimax with strategic prioritization
    /// </summary>
    /// <param name="gameState">Current state of the game board</param>
    /// <returns>The best move for the current player</returns>
    public MoveModel GetBestMove(GameStateModel gameState) {
        CleanCacheIfNeeded();

        var validMoves = BoardUtilities.GetNearbyEmptySpaces(gameState);

        // Handle trivial cases
        var trivialMove = HandleTrivialCases(gameState, validMoves);
        if (trivialMove != null) return trivialMove;

        // Handle opening moves
        var openingMove = HandleOpeningMove(gameState, validMoves);
        if (openingMove != null) return openingMove;

        // Try strategic moves first (winning, blocking, etc.)
        var strategicMove = MoveStrategy.FindBestStrategicMove(gameState, validMoves);
        if (strategicMove != null) return strategicMove;

        // Fall back to minimax search
        return FindBestMoveUsingMinimax(gameState, validMoves);
    }

    /// <summary>
    /// Handles trivial game cases
    /// </summary>
    private static MoveModel? HandleTrivialCases(GameStateModel gameState, List<MoveModel> validMoves) {
        if (validMoves.Count == 0) {
            return new MoveModel {
                Row = GameConstants.BOARD_SIZE / 2,
                Col = GameConstants.BOARD_SIZE / 2
            };
        }

        if (validMoves.Count == 1) {
            return validMoves[0];
        }

        return null;
    }

    /// <summary>
    /// Handles opening move logic
    /// </summary>
    private static MoveModel? HandleOpeningMove(GameStateModel gameState, List<MoveModel> validMoves) {
        if (gameState.Board.Cast<int>().Count(cell => cell != 0) > 2) {
            return null; // Not an opening anymore
        }

        int center = GameConstants.BOARD_SIZE / 2;

        // Take center if available
        if (gameState.Board[center, center] == 0) {
            return new MoveModel { Row = center, Col = center };
        }

        // Otherwise prefer near-center positions
        return validMoves.FirstOrDefault(move =>
            Math.Abs(move.Row - center) <= 2 && Math.Abs(move.Col - center) <= 2);
    }

    /// <summary>
    /// Finds best move using minimax algorithm
    /// </summary>
    private MoveModel FindBestMoveUsingMinimax(GameStateModel gameState, List<MoveModel> validMoves) {
        var moveCache = new Dictionary<string, int>();
        int searchDepth = GetSearchDepth(gameState.Difficulty);

        int bestScore = GameConstants.MIN_SCORE;
        MoveModel? bestMove = null;

        foreach (var move in validMoves) {
            var newState = MakeMove(gameState, move, (int)Player.AI);
            string stateKey = BoardUtilities.GeneratePositionHash(newState, false);

            int score;
            if (moveCache.TryGetValue(stateKey, out int cachedScore)) {
                score = cachedScore;
            } else {
                score = Minimax(newState, searchDepth - 1, GameConstants.MIN_SCORE, GameConstants.MAX_SCORE, false);
                moveCache[stateKey] = score;
            }

            if (score > bestScore) {
                bestScore = score;
                bestMove = move;

                // Early termination for winning moves
                if (score >= GameConstants.MAX_SCORE - 100) {
                    break;
                }
            }
        }

        return bestMove ?? new MoveModel {
            Row = GameConstants.BOARD_SIZE / 2,
            Col = GameConstants.BOARD_SIZE / 2
        };
    }

    /// <summary>
    /// Gets search depth based on difficulty
    /// </summary>
    private static int GetSearchDepth(Utilities.Difficulty difficulty) {
        return difficulty switch {
            Utilities.Difficulty.Easy => GameConstants.SearchDepth.EASY,
            Utilities.Difficulty.Hard => GameConstants.SearchDepth.HARD,
            Utilities.Difficulty.Expert => GameConstants.SearchDepth.EXPERT,
            _ => GameConstants.SearchDepth.MEDIUM
        };
    }

    /// <summary>
    /// Implements minimax algorithm with alpha-beta pruning and caching
    /// </summary>
    private int Minimax(GameStateModel state, int depth, int alpha, int beta, bool isMaximizing) {
        string positionHash = BoardUtilities.GeneratePositionHash(state, isMaximizing);

        // Check cache
        if (_positionCache.TryGetValue(positionHash, out var cachedEntry) && cachedEntry.Depth >= depth) {
            switch (cachedEntry.Type) {
                case TranspositionEntryType.Exact:
                    return cachedEntry.Score;
                case TranspositionEntryType.LowerBound when cachedEntry.Score >= beta:
                    return cachedEntry.Score;
                case TranspositionEntryType.UpperBound when cachedEntry.Score <= alpha:
                    return cachedEntry.Score;
            }
        }

        // Terminal conditions
        if (depth == 0) return BoardEvaluator.EvaluateBoard(state);

        // Check for immediate threats
        var (aiWinningMove, humanWinningMove) = WinChecker.DetectImmediateThreats(state);
        if (aiWinningMove) return GameConstants.MAX_SCORE - (5 - depth);
        if (humanWinningMove) return GameConstants.MIN_SCORE + (5 - depth);

        // Check for critical threats at depth 1
        if (depth == 1) {
            int criticalThreatScore = BoardEvaluator.DetectCriticalThreats(state, isMaximizing);
            if (criticalThreatScore != 0) {
                return criticalThreatScore;
            }
        }

        var validMoves = BoardUtilities.GetNearbyEmptySpaces(state);
        if (validMoves.Count == 0) return 0;

        validMoves = MoveStrategy.PrioritizeCriticalMoves(state, validMoves, isMaximizing);

        int originalAlpha = alpha;
        int bestScore = isMaximizing ? GameConstants.MIN_SCORE : GameConstants.MAX_SCORE;

        foreach (var move in validMoves) {
            var newState = MakeMove(state, move, isMaximizing ? (int)Player.AI : (int)Player.Human);
            var score = Minimax(newState, depth - 1, alpha, beta, !isMaximizing);

            if (isMaximizing) {
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, score);
                if (score >= GameConstants.MAX_SCORE - 10) break; // Early win detection
            } else {
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, score);
                if (score <= GameConstants.MIN_SCORE + 10) break; // Early loss detection
            }

            if (beta <= alpha) break; // Alpha-beta pruning
        }

        // Cache the result
        CachePosition(positionHash, bestScore, depth, originalAlpha, beta);

        return bestScore;
    }

    /// <summary>
    /// Caches position evaluation
    /// </summary>
    private void CachePosition(string positionHash, int bestScore, int depth, int originalAlpha, int beta) {
        TranspositionEntryType entryType;
        if (bestScore <= originalAlpha) {
            entryType = TranspositionEntryType.UpperBound;
        } else if (bestScore >= beta) {
            entryType = TranspositionEntryType.LowerBound;
        } else {
            entryType = TranspositionEntryType.Exact;
        }

        _positionCache[positionHash] = new TranspositionEntry {
            Score = bestScore,
            Depth = depth,
            Type = entryType
        };
    }

    /// <summary>
    /// Cleans cache when it gets too large
    /// </summary>
    private void CleanCacheIfNeeded() {
        if (_positionCache.Count <= GameConstants.CacheSettings.MAX_CACHE_SIZE) return;

        Dictionary<string, TranspositionEntry>? valuableEntries = _positionCache
            .OrderByDescending(entry => entry.Value.Depth)
            .Take(GameConstants.CacheSettings.REDUCED_CACHE_SIZE)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        _positionCache.Clear();
        foreach (var entry in valuableEntries) {
            _positionCache[entry.Key] = entry.Value;
        }
    }

    /// <summary>
    /// Validates if a move is legal
    /// </summary>
    public bool IsValidMove(GameStateModel gameState, MoveModel move) {
        return move.Row >= 0 && move.Row < GameConstants.BOARD_SIZE &&
               move.Col >= 0 && move.Col < GameConstants.BOARD_SIZE &&
               gameState.Board[move.Row, move.Col] == 0;
    }

    /// <summary>
    /// Checks if the last move created a winning condition
    /// </summary>
    public bool CheckWin(GameStateModel gameState, MoveModel lastMove) {
        return WinChecker.CheckWin(gameState, lastMove);
    }

    /// <summary>
    /// Applies a move to the game state
    /// </summary>
    public GameStateModel MakeMove(GameStateModel gameState, MoveModel move, int playerToMove) {
        if (!IsValidMove(gameState, move)) {
            return gameState;
        }

        return new GameStateModel {
            Board = (int[,])gameState.Board.Clone(),
            Difficulty = gameState.Difficulty
        }.ApplyMove(move, playerToMove);
    }
}
