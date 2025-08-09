<<<<<<< HEAD
namespace CelestialLeague.Shared.Models
{
    public class PlayerMatchStats
    {
        public int PlayerId { get; set; }
        public int Deaths { get; set; }
        public int CompletionTimeMs { get; set; }
        public int CheckpointsReached { get; set; }
        public float BestTime { get; set; }
        public bool FinishedLevel { get; set; }
        public Dictionary<string, object> AdditionalStats { get; set; } = new();
    }
}
=======
namespace CelestialLeague.Shared.Models
{
    public class PlayerMatchStats
    {
        public int PlayerId { get; set; }
        public int Deaths { get; set; }
        public int CompletionTimeMs { get; set; }
        public int CheckpointsReached { get; set; }
        public float BestTime { get; set; }
        public bool FinishedLevel { get; set; }
        public Dictionary<string, object> AdditionalStats { get; set; } = new();
    }
}
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
