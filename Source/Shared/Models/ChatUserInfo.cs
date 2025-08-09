<<<<<<< HEAD
using CelestialLeague.Shared.Enums;

public class ChatUserInfo
{
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public UserRole Role { get; set; } = UserRole.None;
    public PlayerStatus Status { get; set; } = PlayerStatus.Online;
    public bool IsMuted { get; set; }
=======
using CelestialLeague.Shared.Enums;

public class ChatUserInfo
{
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public UserRole Role { get; set; } = UserRole.None;
    public PlayerStatus Status { get; set; } = PlayerStatus.Online;
    public bool IsMuted { get; set; }
>>>>>>> 48bc47b13401bb7e2dfc20bc611c767893bc8e52
}