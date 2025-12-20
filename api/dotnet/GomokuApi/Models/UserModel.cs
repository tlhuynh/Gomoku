using System.ComponentModel.DataAnnotations;

namespace GomokuApi.Models;

/// <summary>
/// User entity
/// </summary>
public class UserModel {
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// AzureAD Object ID (unique identifier from Azure)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string AzureADObjectId { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Game Statistics
    public int GamesPlayed { get; set; } = 0;
    public int GamesWon { get; set; } = 0;
    public int GamesLost { get; set; } = 0;
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
}
