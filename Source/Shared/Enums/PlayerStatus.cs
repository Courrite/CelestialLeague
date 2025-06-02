public enum PlayerStatus
{
    Offline, // player is not in game or doesn't have his mod on
    Online, // player is online and has his mod on
    Playing, // player is in an active match
    Spectating, // player is spectating a match
}