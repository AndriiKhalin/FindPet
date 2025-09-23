namespace FindPet.Domain.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException(string message, int statusCode, string errorCode, object? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }

    public int StatusCode { get; }
    public string ErrorCode { get; }
    public object? Details { get; }
}