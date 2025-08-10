namespace CelestialLeague.Shared.Enum
{
    public enum ResponseCode
    {
        // 0-99 general errors
        INVALID_VERSION = 0,
        // 1
        // 2
        FORBIDDEN = 3,
        NOT_FOUND = 4,
        SUCCESS = 5,

        // 100-199 account errors
        ACCOUNT_NOT_FOUND = 100,
        ACCOUNT_INVALID_CREDENTIALS = 101,
        ACCOUNT_INVALID_TOKEN = 102,
        ACCOUNT_NOT_AUTHENTICATED = 103,
        ACCOUNT_USERNAME_TAKEN = 104,
        ACCOUNT_INVALID_USERNAME = 105,
        ACCOUNT_INVALID_PASSWORD = 106,
        ACCOUNT_DISABLED = 107,

        // 200-299 network errors
        NETWORK_RATE_LIMITED = 200,
        NETWORK_INVALID_PACKET = 201,
        NETWORK_SERVER_ERROR = 203,
        NETWORK_CONNECTION_LOST = 204,
        NETWORK_TIMEOUT = 205,
        INTERNAL_ERROR = 206,
        NETWORK_MAINTENANCE = 207,
    }
}