namespace FindPet.Domain.Exceptions;

public class BadRequestException : BaseException
{
    public BadRequestException()
        : base("Bad Request", 400, "BAD_REQUEST")
    {
    }

    public BadRequestException(string message)
        : base(message, 400, "BAD_REQUEST")
    {
    }

    public BadRequestException(string message, int statusCode, string errorCode, object? details = null) : base(message,
        statusCode, errorCode, details)
    {
    }
}