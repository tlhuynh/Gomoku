using GomokuApi.Models;

namespace GomokuApi.Extensions;

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