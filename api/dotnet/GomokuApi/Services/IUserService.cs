using GomokuApi.Models;

namespace GomokuApi.Services;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService {
    /// <summary>
    /// Gets a user by their Azure AD Object ID
    /// </summary>
    Task<UserModel?> GetUserByAzureIdAsync(string azureAdObjectId);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    Task<UserModel?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Creates a new user or returns existing user
    /// </summary>
    Task<UserModel> CreateOrGetUserAsync(string azureAdObjectId, string email, string displayName);

    /// <summary>
    /// Updates user's last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int userId);

    /// <summary>
    /// Updates user's game statistics
    /// </summary>
    Task UpdateGameStatsAsync(int userId, bool won);

    /// <summary>
    /// Gets user by ID
    /// </summary>
    Task<UserModel?> GetUserByIdAsync(int userId);
}
