namespace FindPet.Domain.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException()
        : base("Authentication is required to access this resource", 401, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string message)
        : base(message, 401, "UNAUTHORIZED")
    {
    }
}