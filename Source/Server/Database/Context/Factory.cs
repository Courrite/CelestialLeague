using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CelestialLeague.Server.Database.Context
{
    public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Data Source=celestial_league.db";
            if (args?.Length > 0)
            {
                connectionString = args[0];
            }
            
            return new GameDbContext(connectionString);
        }
    }
}