using GomokuApi.Models;

namespace GomokuApi.Services;

/// <summary>
/// Interface for the Gomoku game service, providing core game mechanics and AI logic
/// </summary>
public interface IGomokuService {
    /// <summary>
    /// Determines the best move for the AI player using minimax algorithm with alpha-beta pruning
    /// </summary>
    /// <param name="gameState">Current state of the game board and player information</param>
    /// <returns>The best move determined by the AI</returns>
    MoveModel GetBestMove(GameStateModel gameState);

    /// <summary>
    /// Checks if a move is valid according to game rules
    /// </summary>
    /// <param name="gameState">Current state of the game</param>
    /// <param name="move">The move to validate</param>
    /// <returns>True if the move is valid, false otherwise</returns>
    bool IsValidMove(GameStateModel gameState, MoveModel move);

    /// <summary>
    /// Applies a move to the game state and returns the new resulting state
    /// </summary>
    /// <param name="gameState">Current state of the game</param>
    /// <param name="move">The move to apply</param>
    /// <param name="playerToMove">Player to make the move</param>
    /// <returns>New game state after the move is applied</returns>
    GameStateModel MakeMove(GameStateModel gameState, MoveModel move, int playerToMove);

    /// <summary>
    /// Checks if the last move created a winning condition (5 or more stones in a row)
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="lastMove">The most recent move that was made</param>
    /// <returns>True if the move created a winning condition, false otherwise</returns>
    public bool CheckWin(GameStateModel gameState, MoveModel lastMove);
}