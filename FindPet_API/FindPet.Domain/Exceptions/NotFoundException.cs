namespace FindPet.Domain.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found", 404, "RESOURCE_NOT_FOUND",
            new { ResourceType = resourceType, ResourceId = resourceId })
    {
    }

    public NotFoundException(string message)
        : base(message, 404, "RESOURCE_NOT_FOUND")
    {
    }
}