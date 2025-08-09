<<<<<<< HEAD
namespace CelestialLeague.Shared.Models
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required int UserId { get; set; }
        public required string Username { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
=======
namespace CelestialLeague.Shared.Models
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required int UserId { get; set; }
        public required string Username { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
}