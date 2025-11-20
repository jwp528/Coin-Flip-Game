using CoinFlipGame.Lib.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinFlipGame.Api.Data;

/// <summary>
/// Database context for the Coin Flip Game
/// </summary>
public class CoinFlipGameDbContext : DbContext
{
    public CoinFlipGameDbContext(DbContextOptions<CoinFlipGameDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Coins collection
    /// </summary>
    public DbSet<Coin> Coins { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure default schema
        modelBuilder.HasDefaultSchema("CFG");

        // Configure Coin entity
        modelBuilder.Entity<Coin>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Indexes for common queries
            entity.HasIndex(e => e.CoinType);
            entity.HasIndex(e => e.Path).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Category);

            // Configure string properties with max length
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.FlavorText)
                .HasMaxLength(500);

            entity.Property(e => e.UnlockDescription)
                .HasMaxLength(500);

            entity.Property(e => e.Path)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.Rarity)
                .HasMaxLength(50);

            // Configure enum as string
            entity.Property(e => e.CoinType)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Configure JSON columns
            entity.Property(e => e.UnlockCriteria)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Prerequisites)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Effects)
                .HasColumnType("nvarchar(max)");

            // Configure default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsAlwaysUnlocked)
                .HasDefaultValue(false);
        });
    }
}
