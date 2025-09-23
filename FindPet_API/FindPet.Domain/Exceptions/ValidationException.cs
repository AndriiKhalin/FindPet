namespace FindPet.Domain.Exceptions;

public class ValidationException : BaseException
{
    public ValidationException(IEnumerable<ValidationError> validationErrors)
        : base("One or more validation errors occurred", 400, "VALIDATION_FAILED", validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string field, string message)
        : this(new[] { new ValidationError(field, message) })
    {
    }

    public ValidationException(string message)
        : base(message, 400, "VALIDATION_FAILED")
    {
        ValidationErrors = Array.Empty<ValidationError>();
    }

    public IEnumerable<ValidationError> ValidationErrors { get; }
}

public record ValidationError(string Field, string Message);