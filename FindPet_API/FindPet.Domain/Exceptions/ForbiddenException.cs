namespace FindPet.Domain.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException()
        : base("You don't have permission to perform this action", 403, "FORBIDDEN")
    {
    }

    public ForbiddenException(string message)
        : base(message, 403, "FORBIDDEN")
    {
    }

    public ForbiddenException(string resource, string action)
        : base($"You don't have permission to {action} {resource}", 403, "FORBIDDEN",
            new { Resource = resource, Action = action })
    {
    }
}