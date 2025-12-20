using GomokuApi.Models;
using GomokuApi.Models.Requests;
using GomokuApi.Utilities;

namespace GomokuApi.Mapping;

/// <summary>
/// Provides mapping functions between frontend and backend data models
/// </summary>
public static class GameStateMapper {
    /// <summary>
    /// Maps a frontend game state request to the backend GameStateModel
    /// </summary>
    /// <param name="request">The frontend request object</param>
    /// <returns>A properly formatted GameStateModel</returns>
    public static GameStateModel MapFromFrontend(GameStateRequestModel request) {
        // Create game state with default values
        GameStateModel gameState = new() {
            Difficulty = MapDifficulty(request.Difficulty),
        };

        // Map the 2D jagged array to rectangular array
        if (request.Board != null) {
            int size = request.Board.Length;
            gameState.Board = new int[size, size];

            for (int i = 0; i < size; i++) {
                for (int j = 0; j < Math.Min(size, request.Board[i].Length); j++) {
                    gameState.Board[i, j] = request.Board[i][j];
                }
            }
        }

        return gameState;
    }

    /// <summary>
    /// Maps a string difficulty to the backend Difficulty enum
    /// </summary>
    private static Difficulty MapDifficulty(string? difficulty) {
        return difficulty?.ToLower() switch {
            "easy" => Difficulty.Easy,
            "hard" => Difficulty.Hard,
            "expert" => Difficulty.Expert,
            _ => Difficulty.Medium // Default to medium
        };
    }
}