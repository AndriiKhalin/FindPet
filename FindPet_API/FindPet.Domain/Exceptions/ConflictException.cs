namespace FindPet.Domain.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message)
        : base(message, 409, "CONFLICT")
    {
    }

    public ConflictException(string resourceType, string conflictReason)
        : base($"Conflict with {resourceType}: {conflictReason}", 409, "CONFLICT",
            new { ResourceType = resourceType, Reason = conflictReason })
    {
    }
}