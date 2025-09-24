using System.Net;
using System.Text.Json;
using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;
using FindPet.Domain.Exceptions;
using FindPet.WebApi.Models.Exceptions;

namespace FindPet.WebApi.Middlewares;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class ExceptionHandlingMiddleware
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILoggerManager _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILoggerManager logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);

            await HandleHttpStatusCodesAsync(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleHttpStatusCodesAsync(HttpContext context)
    {
        // Only handle error status codes that haven't been processed yet
        if (context.Response.StatusCode >= 400 && !context.Response.HasStarted &&
            (context.Response.ContentLength == null || context.Response.ContentLength == 0))
        {
            var errorResponse = CreateErrorResponseForStatusCode(context.Response.StatusCode, context.TraceIdentifier);

            context.Response.ContentType = "application/json";

            LogException(errorResponse.StatusCode, null, errorResponse.Message);

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    private ErrorResponse CreateErrorResponseForStatusCode(int statusCode, string traceId)
    {
        return statusCode switch
        {
            401 => ErrorResponse.Create(
                "Authentication is required to access this resource. Please provide a valid JWT token.",
                HttpStatusCode.Unauthorized,
                "AUTHENTICATION_REQUIRED",
                traceId,
                new
                {
                    Hint = "Include 'Authorization: Bearer <your-jwt-token>' in the request headers",
                    LoginEndpoint = "/api/Account/login"
                }),

            403 => ErrorResponse.Create(
                "You don't have permission to access this resource",
                HttpStatusCode.Forbidden,
                "INSUFFICIENT_PERMISSIONS",
                traceId),

            404 => ErrorResponse.Create(
                "The requested resource was not found",
                HttpStatusCode.NotFound,
                "RESOURCE_NOT_FOUND",
                traceId),

            405 => ErrorResponse.Create(
                "The HTTP method is not allowed for this resource",
                HttpStatusCode.MethodNotAllowed,
                "METHOD_NOT_ALLOWED",
                traceId),

            415 => ErrorResponse.Create(
                "The media type is not supported",
                HttpStatusCode.UnsupportedMediaType,
                "UNSUPPORTED_MEDIA_TYPE",
                traceId),

            _ => ErrorResponse.Create(
                "An error occurred while processing your request",
                (HttpStatusCode)statusCode,
                "HTTP_ERROR",
                traceId)
        };
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = CreateErrorResponse(exception, context.TraceIdentifier);
        context.Response.StatusCode = errorResponse.StatusCode;

        LogException(errorResponse.StatusCode, exception);

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponse CreateErrorResponse(Exception exception, string traceId)
    {
        return exception switch
        {
            // Our custom exceptions - use their properties
            ValidationException validationEx => ErrorResponse.CreateValidationError(validationEx, traceId),
            BaseException baseEx => ErrorResponse.CreateFromException(baseEx, traceId),


            // Standard .NET exceptions
            UnauthorizedAccessException => ErrorResponse.Create(
                "Authentication required",
                HttpStatusCode.Unauthorized,
                "UNAUTHORIZED",
                traceId),

            ArgumentNullException argNullEx => ErrorResponse.Create(
                $"Required parameter '{argNullEx.ParamName}' is missing",
                HttpStatusCode.BadRequest,
                "MISSING_PARAMETER",
                traceId,
                new { Parameter = argNullEx.ParamName }),

            ArgumentException argEx => ErrorResponse.Create(
                "Invalid input provided",
                HttpStatusCode.BadRequest,
                "INVALID_INPUT",
                traceId,
                _environment.IsDevelopment()
                    ? new { Parameter = argEx.ParamName, OriginalMessage = argEx.Message }
                    : null),

            InvalidOperationException invalidOpEx => ErrorResponse.Create(
                "The requested operation cannot be performed",
                HttpStatusCode.BadRequest,
                "INVALID_OPERATION",
                traceId,
                _environment.IsDevelopment() ? new { OriginalMessage = invalidOpEx.Message } : null),

            TimeoutException => ErrorResponse.Create(
                "Operation timed out",
                HttpStatusCode.RequestTimeout,
                "TIMEOUT",
                traceId),

            TaskCanceledException => ErrorResponse.Create(
                "Operation was cancelled",
                HttpStatusCode.RequestTimeout,
                "OPERATION_CANCELLED",
                traceId),

            NotSupportedException notSupportedEx => ErrorResponse.Create(
                "Operation not supported",
                HttpStatusCode.NotImplemented,
                "NOT_SUPPORTED",
                traceId,
                _environment.IsDevelopment() ? new { OriginalMessage = notSupportedEx.Message } : null),

            FileNotFoundException fileNotFoundEx => ErrorResponse.Create(
                "Required file was not found",
                HttpStatusCode.NotFound,
                "FILE_NOT_FOUND",
                traceId,
                new { fileNotFoundEx.FileName }),

            DirectoryNotFoundException => ErrorResponse.Create(
                "Required directory was not found",
                HttpStatusCode.NotFound,
                "DIRECTORY_NOT_FOUND",
                traceId),

            OutOfMemoryException => ErrorResponse.Create(
                "Server is out of memory",
                HttpStatusCode.InternalServerError,
                "OUT_OF_MEMORY",
                traceId),

            StackOverflowException => ErrorResponse.Create(
                "Stack overflow occurred",
                HttpStatusCode.InternalServerError,
                "STACK_OVERFLOW",
                traceId),

            FormatException formatEx => ErrorResponse.Create(
                "Invalid data format",
                HttpStatusCode.BadRequest,
                "INVALID_FORMAT",
                traceId,
                _environment.IsDevelopment() ? new { OriginalMessage = formatEx.Message } : null),

            OverflowException => ErrorResponse.Create(
                "Numeric overflow occurred",
                HttpStatusCode.BadRequest,
                "NUMERIC_OVERFLOW",
                traceId),

            DivideByZeroException => ErrorResponse.Create(
                "Division by zero attempted",
                HttpStatusCode.BadRequest,
                "DIVISION_BY_ZERO",
                traceId),

            IndexOutOfRangeException => ErrorResponse.Create(
                "Index was outside the bounds of the array",
                HttpStatusCode.BadRequest,
                "INDEX_OUT_OF_RANGE",
                traceId),

            KeyNotFoundException keyNotFoundEx => ErrorResponse.Create(
                "Required key was not found",
                HttpStatusCode.NotFound,
                "KEY_NOT_FOUND",
                traceId,
                _environment.IsDevelopment() ? new { OriginalMessage = keyNotFoundEx.Message } : null),

            NullReferenceException nullRefEx => ErrorResponse.Create(
                "Null reference encountered",
                HttpStatusCode.InternalServerError,
                "NULL_REFERENCE",
                traceId,
                _environment.IsDevelopment()
                    ? new { OriginalMessage = nullRefEx.Message, StackTrace = nullRefEx.StackTrace }
                    : null),

            // Fallback for all other exceptions
            _ => ErrorResponse.Create(
                _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred",
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                traceId,
                _environment.IsDevelopment()
                    ? new
                    {
                        ExceptionType = exception.GetType().Name,
                        OriginalMessage = exception.Message,
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException?.Message
                    }
                    : null)
        };
    }

    private void LogException(int statusCode, Exception? exception = null, string? message = null)
    {
        if (exception is not null && message is null)
        {
            message = $"Exception: {exception.GetType().Name} - {exception.Message}";
        }

        switch (statusCode)
        {
            case >= 500:
                _logger.LogError(message);
                break;
            case >= 400:
                _logger.LogWarn(message);
                break;
            default:
                _logger.LogInfo(message);
                break;
        }
    }
}