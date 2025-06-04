namespace CelestialLeague.Shared.Enums
{
    public enum ResponseErrorCode

    {
        // general (1-99)
        Unknown = 1,
        InvalidRequest = 1,
        InvalidPacket = 2,
        RateLimited = 4,
        ServerError = 5,

        // auth (100-199)
        InvalidCredentials = 100,
        AccountNotFound = 101,
        AccountAlreadyExists = 102,
        SessionExpired = 103,
        SessionInvalid = 104,
        UsernameInvalid = 105,
        EmailInvalid = 106,

        // matchmaking (200-299)
        AlreadyInQueue = 200,
        NotInQueue = 201,
        MatchNotFound = 202,
        Matchfull = 203,
        MatchAlreadyStarted = 204,
        InsufficientRank = 205,

        // game (300-399)
        GameNotFound = 300,
        GameFull = 301,
        GameAlreadyStarted = 302,
        PlayerNotInGame = 303,
        InvalidGameAction = 304,

        // chat
        MessageTooLong = 400,
        ChannelNotFound = 401,
        NotInChannel = 402,
        InsufficientPermissions = 403,
        PlayerMuted = 404,

        // network (500-599)
        ConnectionLost = 500,
        Timeout = 501,
        InvalidVersion = 502,
        ServerMaintenance = 503
    }
}