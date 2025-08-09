<<<<<<< HEAD
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
=======
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
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
}