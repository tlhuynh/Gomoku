using GomokuApi.Constants;
using GomokuApi.Extensions;
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
        // Check each priority level in order - prioritize defense when necessary
        var priorities = new[] {
            MovePriority.ImmediateWin,        // 1. Make our immediate win move
            MovePriority.BlockWin,            // 2. Block their immediate win move  
            MovePriority.GuaranteedWin,       // 3. Make our guaranteed win move (because they have to block)
            MovePriority.BlockGuaranteedWin,  // 4. Block their guaranteed winning move
            MovePriority.OpenThree            // 5. Make our win threat move (OpenThree covers threat creation)
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
            MovePriority.BlockWin => FindBlockImmediateWinMove(gameState, validMoves),
            MovePriority.BlockGuaranteedWin => FindBlockGuaranteedWinMove(gameState, validMoves),
            MovePriority.OpenThree => FindOpenThreeMove(gameState, validMoves),
            _ => null
        };
    }

    /// <summary>
    /// Finds immediate winning move
    /// <param name="gameState">Current game state</param>
    /// <param name="validMoves">List of valid moves</param>
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
    /// <param name="gameState">Current game state</param>
    /// <param name="validMoves">List of valid moves</param>
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
    /// Finds move that blocks opponent's immediate win
    /// <param name="gameState">Current game state</param>
    /// <param name="validMoves">List of valid moves</param>
    /// </summary>
    private static MoveModel? FindBlockImmediateWinMove(GameStateModel gameState, List<MoveModel> validMoves) {
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
    /// <param name="gameState">Current game state</param>
    /// <param name="validMoves">List of valid moves</param>
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
    /// Finds move that creates strong open three or blocks opponent threats
    /// <param name="gameState">Current game state</param>
    /// <param name="validMoves">List of valid moves</param>
    /// </summary>
    private static MoveModel? FindOpenThreeMove(GameStateModel gameState, List<MoveModel> validMoves) {
        // First, try to create our own open three
        foreach (var move in validMoves) {
            var newState = MakeTempMove(gameState, move, Player.AI);
            if (PatternAnalyzer.CheckOpenThreeFormation(newState, move, Player.AI)) {
                return move;
            }
        }

        // If no strong offensive move, block opponent's open three formations
        foreach (var move in validMoves) {
            var tempState = MakeTempMove(gameState, move, Player.Human);
            if (PatternAnalyzer.CheckOpenThreeFormation(tempState, move, Player.Human)) {
                return move; // Block their threat by taking their spot
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
        List<MoveModel> winningMoves = [];
        List<MoveModel> blockingMoves = [];
        List<MoveModel> guaranteedWinMoves = [];
        List<MoveModel> blockGuaranteedMoves = [];
        List<MoveModel> threatMoves = [];
        List<MoveModel> normalMoves = [];

        var currentPlayer = isMaximizing ? Player.AI : Player.Human;
        var opponent = currentPlayer == Player.AI ? Player.Human : Player.AI;

        foreach (var move in moves) {
            // 1. Check for immediate winning moves
            var tempStateWin = MakeTempMove(state, move, currentPlayer);
            if (WinChecker.CheckWinAtPosition(tempStateWin, move.Row, move.Col)) {
                winningMoves.Add(move);
                continue;
            }

            // 2. Check for blocking opponent's immediate win
            var tempStateBlock = MakeTempMove(state, move, opponent);
            if (WinChecker.CheckWinAtPosition(tempStateBlock, move.Row, move.Col)) {
                blockingMoves.Add(move);
                continue;
            }

            // 3. Check for guaranteed win setups
            if (PatternAnalyzer.CheckGuaranteedWin(tempStateWin, move, currentPlayer)) {
                guaranteedWinMoves.Add(move);
                continue;
            }

            // 4. Check for blocking opponent's guaranteed win setups
            if (PatternAnalyzer.CheckGuaranteedWin(tempStateBlock, move, opponent)) {
                blockGuaranteedMoves.Add(move);
                continue;
            }

            // 5. Check for threat creation or blocking
            if (PatternAnalyzer.CheckOpenThreeFormation(tempStateWin, move, currentPlayer) ||
                PatternAnalyzer.CheckOpenThreeFormation(tempStateBlock, move, opponent)) {
                threatMoves.Add(move);
                continue;
            }

            // 6. Normal moves
            normalMoves.Add(move);
        }

        // Return moves in priority order
        List<MoveModel> prioritizedMoves = [];
        prioritizedMoves.AddRange(winningMoves);      // Highest priority
        prioritizedMoves.AddRange(blockingMoves);     // Block immediate threats
        prioritizedMoves.AddRange(guaranteedWinMoves); // Setup guaranteed wins
        prioritizedMoves.AddRange(blockGuaranteedMoves); // Block opponent setups
        prioritizedMoves.AddRange(threatMoves);       // Create/block threats
        prioritizedMoves.AddRange(normalMoves);       // Everything else

        return prioritizedMoves;
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