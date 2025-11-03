using GomokuApi.Constants;
using GomokuApi.Models;

namespace GomokuApi.Services.Helpers;

/// <summary>
/// Handles move prioritization and strategy
/// </summary>
public static class MoveStrategy {
    /// <summary>
    /// Different types of move priorities
    /// </summary>
    public enum MovePriority {
        ImmediateWin,
        GuaranteedWin,
        BlockWin,
        BlockGuaranteedWin,
        OpenThree,
        Normal
    }

    /// <summary>
    /// Move with its priority information
    /// </summary>
    public record PrioritizedMove(MoveModel Move, MovePriority Priority, int Score = 0);

    /// <summary>
    /// Finds the best move using prioritized strategies
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="validMoves">Valid moves to consider</param>
    /// <returns>Best move or null if none found</returns>
    public static MoveModel? FindBestStrategicMove(GameStateModel gameState, List<MoveModel> validMoves) {
        // Check each priority level in order
        var priorities = new[] {
            MovePriority.ImmediateWin,
            MovePriority.GuaranteedWin,
            MovePriority.BlockWin,
            MovePriority.BlockGuaranteedWin,
            MovePriority.OpenThree
        };

        foreach (var priority in priorities) {
            var move = FindMoveByPriority(gameState, validMoves, priority);
            if (move != null) return move;
        }

        return null; // No strategic move found
    }

    /// <summary>
    /// Finds move by specific priority type
    /// </summary>
    /// <param name="gameState">Game state</param>
    /// <param name="validMoves">Valid moves</param>
    /// <param name="priority">Priority type</param>
    /// <returns>Move if found</returns>
    private static MoveModel? FindMoveByPriority(GameStateModel gameState, List<MoveModel> validMoves, MovePriority priority) {
        return priority switch {
            MovePriority.ImmediateWin => FindImmediateWinMove(gameState, validMoves),
            MovePriority.GuaranteedWin => FindGuaranteedWinMove(gameState, validMoves),
            MovePriority.BlockWin => FindBlockingMove(gameState, validMoves),
            MovePriority.BlockGuaranteedWin => FindBlockGuaranteedWinMove(gameState, validMoves),
            MovePriority.OpenThree => FindOpenThreeMove(gameState, validMoves),
            _ => null
        };
    }

    /// <summary>
    /// Finds immediate winning move
    /// </summary>
    private static MoveModel? FindImmediateWinMove(GameStateModel gameState, List<MoveModel> validMoves) {
        foreach (var move in validMoves) {
            var newState = MakeTempMove(gameState, move, Player.AI);
            if (WinChecker.CheckWin(newState, move)) {
                return move;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds guaranteed winning move
    /// </summary>
    private static MoveModel? FindGuaranteedWinMove(GameStateModel gameState, List<MoveModel> validMoves) {
        foreach (var move in validMoves) {
            var newState = MakeTempMove(gameState, move, Player.AI);
            if (PatternAnalyzer.CheckGuaranteedWin(newState, move, Player.AI)) {
                return move;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds move that blocks opponent's win
    /// </summary>
    private static MoveModel? FindBlockingMove(GameStateModel gameState, List<MoveModel> validMoves) {
        foreach (var move in validMoves) {
            var tempState = MakeTempMove(gameState, move, Player.Human);
            if (WinChecker.CheckWin(tempState, move)) {
                return move;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds move that blocks opponent's guaranteed win
    /// </summary>
    private static MoveModel? FindBlockGuaranteedWinMove(GameStateModel gameState, List<MoveModel> validMoves) {
        foreach (var move in validMoves) {
            var tempState = MakeTempMove(gameState, move, Player.Human);
            if (PatternAnalyzer.CheckGuaranteedWin(tempState, move, Player.Human)) {
                return move;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds move that creates strong open three
    /// </summary>
    private static MoveModel? FindOpenThreeMove(GameStateModel gameState, List<MoveModel> validMoves) {
        foreach (var move in validMoves) {
            var newState = MakeTempMove(gameState, move, Player.AI);
            if (PatternAnalyzer.CheckOpenThreeFormation(newState, move, Player.AI)) {
                return move;
            }
        }
        return null;
    }

    /// <summary>
    /// Prioritizes critical moves for alpha-beta pruning optimization
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="moves">Moves to prioritize</param>
    /// <param name="isMaximizing">Whether maximizing player</param>
    /// <returns>Prioritized moves</returns>
    public static List<MoveModel> PrioritizeCriticalMoves(GameStateModel state, List<MoveModel> moves, bool isMaximizing) {
        var criticalMoves = new List<MoveModel>();
        var currentPlayer = isMaximizing ? Player.AI : Player.Human;

        // Find winning moves first
        foreach (var move in moves) {
            var tempState = MakeTempMove(state, move, currentPlayer);
            if (WinChecker.CheckWinAtPosition(tempState, move.Row, move.Col)) {
                criticalMoves.Add(move);
            }
        }

        if (criticalMoves.Count > 0) return criticalMoves;

        // Find blocking moves
        var opponent = currentPlayer == Player.AI ? Player.Human : Player.AI;
        foreach (var move in moves) {
            var tempState = MakeTempMove(state, move, opponent);
            if (WinChecker.CheckWinAtPosition(tempState, move.Row, move.Col)) {
                criticalMoves.Add(move);
            }
        }

        if (criticalMoves.Count > 0) {
            var prioritizedMoves = new List<MoveModel>(moves);
            foreach (var move in criticalMoves) {
                prioritizedMoves.RemoveAll(m => m.Row == move.Row && m.Col == move.Col);
            }
            prioritizedMoves.InsertRange(0, criticalMoves);
            return prioritizedMoves;
        }

        return moves;
    }

    /// <summary>
    /// Creates temporary move for testing
    /// </summary>
    /// <param name="gameState">Original state</param>
    /// <param name="move">Move to make</param>
    /// <param name="player">Player making move</param>
    /// <returns>New game state</returns>
    private static GameStateModel MakeTempMove(GameStateModel gameState, MoveModel move, Player player) {
        return new GameStateModel {
            Board = (int[,])gameState.Board.Clone(),
            Difficulty = gameState.Difficulty
        }.ApplyMove(move, (int)player);
    }
}

/// <summary>
/// Extension methods for GameStateModel
/// </summary>
public static class GameStateExtensions {
    /// <summary>
    /// Applies a move to the game state
    /// </summary>
    /// <param name="state">Game state</param>
    /// <param name="move">Move to apply</param>
    /// <param name="player">Player making the move</param>
    /// <returns>Modified game state</returns>
    public static GameStateModel ApplyMove(this GameStateModel state, MoveModel move, int player) {
        state.Board[move.Row, move.Col] = player;
        return state;
    }
}