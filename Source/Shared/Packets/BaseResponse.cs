using CelestialLeague.Shared.Utils;
using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared;

namespace CelestialLeague.Shared.Packets
{
    public abstract class BaseResponse : BasePacket
    {
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public ResponseErrorCode? ErrorCode { get; set; }

        protected BaseResponse(bool success = true)
        {
            Success = success;
            TimeStamp = DateTime.UtcNow;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(CorrelationId))
                return false;

            if (!Success)
            {
                return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                       ErrorCode.HasValue &&
                       Enum.IsDefined(typeof(ResponseErrorCode), ErrorCode.Value);
            }

            return ValidateSuccessResponse();
        }

        // override in derived classes for success-specific validation
        protected virtual bool ValidateSuccessResponse()
        {
            return true;
        }

        // error categorization methods
        public bool IsAuthenticationError()
        {
            return !Success && ErrorCode.HasValue && (
                ErrorCode == ResponseErrorCode.InvalidCredentials ||
                ErrorCode == ResponseErrorCode.AccountNotFound ||
                ErrorCode == ResponseErrorCode.SessionExpired ||
                ErrorCode == ResponseErrorCode.SessionInvalid
            );
        }

        public bool IsServerError()
        {
            return !Success && ErrorCode.HasValue && (
                ErrorCode == ResponseErrorCode.ServerMaintenance ||
                ErrorCode == ResponseErrorCode.ServerError ||
                ErrorCode == ResponseErrorCode.ConnectionLost ||
                ErrorCode == ResponseErrorCode.Timeout
            );
        }

        public bool IsClientError()
        {
            return !Success && ErrorCode.HasValue && (
                ErrorCode == ResponseErrorCode.InvalidVersion ||
                ErrorCode == ResponseErrorCode.InvalidRequest ||
                ErrorCode == ResponseErrorCode.RateLimited ||
                ErrorCode == ResponseErrorCode.InvalidPacket
            );
        }

        public bool ShouldRetry()
        {
            return !Success && ErrorCode.HasValue && (
                ErrorCode == ResponseErrorCode.RateLimited ||
                ErrorCode == ResponseErrorCode.ServerError ||
                ErrorCode == ResponseErrorCode.ConnectionLost ||
                ErrorCode == ResponseErrorCode.Timeout
            );
        }

        // factory methods for common error responses
        public static T CreateError<T>(string correlationId, string errorMessage, ResponseErrorCode errorCode)
            where T : BaseResponse, new()
        {
            return new T()
            {
                CorrelationId = correlationId,
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                TimeStamp = DateTime.UtcNow
            };
        }

        public static T CreateAuthError<T>(string correlationId, string message = "Authentication failed")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, ResponseErrorCode.InvalidCredentials);
        }

        public static T CreateServerError<T>(string correlationId, string message = "Internal server error")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, ResponseErrorCode.ServerError);
        }

        public static T CreateValidationError<T>(string correlationId, string message = "Invalid request data")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, ResponseErrorCode.InvalidRequest);
        }

        public static T CreateNotFoundError<T>(string correlationId, string message = "Resource not found")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, ResponseErrorCode.AccountNotFound);
        }

        public static T CreateRateLimitError<T>(string correlationId, string message = "Rate limit exceeded")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, ResponseErrorCode.RateLimited);
        }
    }
}
