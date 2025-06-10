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
            modelBuilder?.Entity<Player>().HasIndex(p => p.Username).IsUnique();
            modelBuilder?.Entity<Player>().Property(p => p.Username).HasMaxLength(GameConstants.MaxUsernameLength);
        }
    }
}