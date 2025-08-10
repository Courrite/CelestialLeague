using CelestialLeague.Shared.Utils;
using CelestialLeague.Shared;
using CelestialLeague.Shared.Enum;

namespace CelestialLeague.Shared.Packets
{
    public abstract class BaseResponse : BasePacket
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public ResponseCode? ResponseCode { get; set; }
        public string? Message { get; set; }

        protected BaseResponse(bool success = true) : base()
        {
            Success = success;
        }

        public override bool IsValid()
        {
            if (CorrelationId < 0)
                return false;

            if (!Success)
            {
                return !string.IsNullOrWhiteSpace(ErrorMessage) &&
                       ResponseCode.HasValue &&
                       System.Enum.IsDefined(typeof(ResponseCode), ResponseCode.Value);
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
            return !Success && ResponseCode.HasValue && (
                ResponseCode == Enum.ResponseCode.ACCOUNT_INVALID_CREDENTIALS ||
                ResponseCode == Enum.ResponseCode.ACCOUNT_NOT_FOUND
            );
        }

        public bool IsServerError()
        {
            return !Success && ResponseCode.HasValue && (
                ResponseCode == Enum.ResponseCode.NETWORK_MAINTENANCE ||
                ResponseCode == Enum.ResponseCode.INTERNAL_ERROR ||
                ResponseCode == Enum.ResponseCode.NETWORK_CONNECTION_LOST ||
                ResponseCode == Enum.ResponseCode.NETWORK_TIMEOUT
            );
        }

        public bool IsClientError()
        {
            return !Success && ResponseCode.HasValue && (
                ResponseCode == Enum.ResponseCode.INVALID_VERSION ||
                ResponseCode == Enum.ResponseCode.NETWORK_INVALID_PACKET ||
                ResponseCode == Enum.ResponseCode.NETWORK_RATE_LIMITED ||
                ResponseCode == Enum.ResponseCode.NETWORK_INVALID_PACKET
            );
        }

        public bool ShouldRetry()
        {
            return !Success && ResponseCode.HasValue && (
                ResponseCode == Enum.ResponseCode.NETWORK_CONNECTION_LOST ||
                ResponseCode == Enum.ResponseCode.NETWORK_TIMEOUT
            );
        }

        // factory methods for common error responses
        public static T CreateError<T>(uint correlationId, string errorMessage, ResponseCode errorCode)
            where T : BaseResponse, new()
        {
            return new T()
            {
                CorrelationId = correlationId,
                Success = false,
                ErrorMessage = errorMessage,
                ResponseCode = errorCode,
            };
        }

        public static T CreateServerError<T>(uint correlationId, string message = "Internal server error")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, Enum.ResponseCode.INTERNAL_ERROR);
        }

        public static T CreateValidationError<T>(uint correlationId, string message = "Invalid request data")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, Enum.ResponseCode.NETWORK_INVALID_PACKET);
        }

        public static T CreateNotFoundError<T>(uint correlationId, string message = "Resource not found")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, Enum.ResponseCode.NOT_FOUND);
        }

        public static T CreateRateLimitError<T>(uint correlationId, string message = "Rate limit exceeded")
            where T : BaseResponse, new()
        {
            return CreateError<T>(correlationId, message, Enum.ResponseCode.NETWORK_RATE_LIMITED);
        }
    }
}

