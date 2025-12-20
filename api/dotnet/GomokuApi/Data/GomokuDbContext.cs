using GomokuApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GomokuApi.Data;

public class GomokuDbContext(DbContextOptions<GomokuDbContext> options) : DbContext(options) {
    public DbSet<UserModel> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<UserModel>(entity => {
            entity.HasIndex(e => e.AzureADObjectId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}