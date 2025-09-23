namespace FindPet.Domain.Exceptions;

public class ExternalServiceException : BaseException
{
    public ExternalServiceException(string serviceName, string message)
        : base($"External service '{serviceName}' failed: {message}", 502, "EXTERNAL_SERVICE_ERROR",
            new { ServiceName = serviceName })
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}