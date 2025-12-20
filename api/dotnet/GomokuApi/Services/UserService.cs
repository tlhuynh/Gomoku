using GomokuApi.Data;
using GomokuApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GomokuApi.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : IUserService {
    private readonly GomokuDbContext _context;

    public UserService(GomokuDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Gets a user by their Azure AD Object ID
    /// </summary>
    public async Task<UserModel?> GetUserByAzureIdAsync(string azureAdObjectId) {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AzureADObjectId == azureAdObjectId);
    }

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    public async Task<UserModel?> GetUserByEmailAsync(string email) {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Creates a new user or returns existing user
    /// </summary>
    public async Task<UserModel> CreateOrGetUserAsync(string azureAdObjectId, string email, string displayName) {
        UserModel? existingUser = await GetUserByAzureIdAsync(azureAdObjectId);

        if (existingUser != null) {
            // Update last login
            existingUser.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingUser;
        }

        // Create new user
        UserModel newUser = new UserModel {
            AzureADObjectId = azureAdObjectId,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return newUser;
    }

    /// <summary>
    /// Updates user's last login timestamp
    /// </summary>
    public async Task UpdateLastLoginAsync(int userId) {
        UserModel? user = await _context.Users.FindAsync(userId);
        if (user != null) {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Updates user's game statistics
    /// </summary>
    public async Task UpdateGameStatsAsync(int userId, bool won) {
        UserModel? user = await _context.Users.FindAsync(userId);
        if (user == null) {
            return;
        }

        user.GamesPlayed++;

        if (won) {
            user.GamesWon++;
            user.CurrentStreak++;

            if (user.CurrentStreak > user.BestStreak) {
                user.BestStreak = user.CurrentStreak;
            }
        } else {
            user.GamesLost++;
            user.CurrentStreak = 0;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets user by ID
    /// </summary>
    public async Task<UserModel?> GetUserByIdAsync(int userId) {
        return await _context.Users.FindAsync(userId);
    }
}
