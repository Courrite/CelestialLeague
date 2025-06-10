using CelestialLeague.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Source.Server.Database.Context
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=celestial_league.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigurePlayer(modelBuilder);
        }

        private static void ConfigurePlayer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>(entity =>
                {
                    entity.HasIndex(p => p.Username).IsUnique().HasDatabaseName("IX_Players_Username");
                    entity.Property(p => p.Username).HasMaxLength(GameConstants.MaxUsernameLength).IsRequired();
                }
            );
        }
    }
}