namespace CelestialLeague.Shared.Enums
{
    public enum PlayerStatus
    {
        Offline = 1, // player is not in game or doesn't have his mod on
        Online = 2, // player is online and has his mod on
        InQueue = 3, // player is online and is waiting for a match
        Playing = 4, // player is in an active match
        Spectating = 5, // player is spectating a match
    }
}