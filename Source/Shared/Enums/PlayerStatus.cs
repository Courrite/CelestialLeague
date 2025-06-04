namespace CelestialLeague.Shared.Enums
{
    public enum PlayerStatus
    {
        Offline = 1, // player is not in game or doesn't have his mod on
        Online = 2, // player is online and has his mod on
        Playing = 3, // player is in an active match
        Spectating = 4, // player is spectating a match
    }
}