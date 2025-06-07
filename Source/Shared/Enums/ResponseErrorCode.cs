namespace CelestialLeague.Shared.Enums
{
    public enum ResponseErrorCode
    {
        // general (1-99)
        Unknown = 1,
        InvalidRequest = 2,
        InvalidPacket = 3,
        RateLimited = 4,
        InternalError = 5,
        ServerError = 6,

        // auth (100-199)
        InvalidCredentials = 100,
        AccountNotFound = 101,
        UsernameExists = 102,
        AccountAlreadyExists = 103,
        SessionExpired = 104,
        InvalidSession = 105,
        NotAuthenticated = 106,
        UsernameInvalid = 107,
        EmailInvalid = 108,

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

        // chat (400-499)
        MessageTooLong = 400,
        ChannelNotFound = 401,
        NotInChannel = 402,
        InsufficientPermissions = 403,
        PlayerMuted = 404,
        MessageFiltered = 405,
        UserNotFound = 406,

        // network (500-599)
        ConnectionLost = 500,
        Timeout = 501,
        InvalidVersion = 502,
        ServerMaintenance = 503,
        InvalidOperation = 504,
    }
}
