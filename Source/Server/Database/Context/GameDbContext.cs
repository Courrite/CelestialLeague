using CelestialLeague.Server.Models;
using CelestialLeague.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace CelestialLeague.Server.Database.Context
{
    public class GameDbContext : DbContext
    {
        private readonly string _connectionString;

        public GameDbContext(string connectionString)
        {
            _connectionString = connectionString ?? "Data Source=celestial_league.db";
        }

        public DbSet<Player> Players { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));
            
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(_connectionString, options =>
                {
                    options.CommandTimeout(30);
                });
                
                #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging(true);
                optionsBuilder.EnableDetailedErrors(true);
                #else
                optionsBuilder.EnableSensitiveDataLogging(false);
                #endif
                
                optionsBuilder.EnableServiceProviderCaching();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
            
            ConfigurePlayer(modelBuilder);
        }

        private static void ConfigurePlayer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.HasIndex(p => p.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Players_Username");
                
                entity.Property(p => p.Username)
                    .HasMaxLength(GameConstants.MaxUsernameLength)
                    .IsRequired()
                    .UseCollation("NOCASE");
                
                entity.Property(p => p.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();
                
                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("datetime('now')")
                    .ValueGeneratedOnAdd();
                
                if (typeof(Player).GetProperty("PlayerStatus") != null)
                {
                    entity.Property(p => p.PlayerStatus)
                        .HasConversion<string>()
                        .HasMaxLength(20);
                }
            });
        }
    }
}