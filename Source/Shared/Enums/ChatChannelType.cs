namespace CelestialLeague.Shared.Enums
{
    public enum ChatChannelType
    {
        // system channels
        Global = 0, // server-wide chat
        General = 1, // main lobby chat
        Help = 2, // help/support channel
        Announcements = 3,  // server announcements (read-only)
        
        // game
        Match = 10, // in-game match chat
        Spectator = 11, // spectator chat during matches
        PostMatch = 12, // post-game chat
        
        // socials
        Private = 20, // direct messages between players
        Party = 21, 
        
        // ranked stuff
        RankedGeneral = 30, // gen ranked discussion
        RankedTips = 31, // strategy or tips
        
        // mod
        ModeratorOnly = 40, // self-explanatory lmao
        Reports = 41, // reports
    }
}
