namespace CelestialLeague.Shared.Enums
{
    public enum AuthResult
    {
        Success = 0,
        InvalidCredentials = 1,
        UsernameTaken = 2,
        InvalidUsername = 3,
        TooManyAttempts = 4,
        SessionExpired = 5,
        DatabaseError = 6,
        UnknownError = 7,
        AccountLocked = 8,
        Timeout = 9,
    }
}