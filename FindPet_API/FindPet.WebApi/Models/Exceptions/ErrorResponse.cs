using FindPet.Domain.Exceptions;
using System.Net;

namespace FindPet.WebApi.Models.Exceptions;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public object? Details { get; set; }
    public IEnumerable<ValidationError>? ValidationErrors { get; set; }

    public static ErrorResponse Create(string message, HttpStatusCode statusCode, string errorCode,
        string? traceId = null, object? details = null)
    {
        return new ErrorResponse
        {
            Message = message,
            StatusCode = (int)statusCode,
            ErrorCode = errorCode,
            TraceId = traceId,
            Details = details
        };
    }

    public static ErrorResponse CreateFromException(BaseException exception, string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = exception.Message,
            StatusCode = exception.StatusCode,
            ErrorCode = exception.ErrorCode,
            TraceId = traceId,
            Details = exception.Details
        };
    }

    //public static ErrorResponse CreateValidationError(IEnumerable<ValidationError> validationErrors,
    //    string? traceId = null)
    //{
    //    return new ErrorResponse
    //    {
    //        Message = "One or more validation errors occurred",
    //        StatusCode = (int)HttpStatusCode.BadRequest,
    //        ErrorCode = "VALIDATION_FAILED",
    //        TraceId = traceId,
    //        ValidationErrors = validationErrors
    //    };
    //}

    public static ErrorResponse CreateValidationError(ValidationException exception, string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = exception.Message,
            StatusCode = (int)HttpStatusCode.BadRequest,
            ErrorCode = "VALIDATION_FAILED",
            TraceId = traceId,
            ValidationErrors = exception.ValidationErrors
        };
    }
}