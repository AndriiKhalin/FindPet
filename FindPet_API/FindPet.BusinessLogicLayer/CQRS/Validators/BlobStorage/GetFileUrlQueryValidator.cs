using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class GetFileUrlQueryValidator : AbstractValidator<GetFileUrlQuery>
{
    public GetFileUrlQueryValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("File Path is required")
            .Must(BeValidFilePath)
            .WithMessage("File name contains invalid characters");

        RuleFor(x => x.ExpiryHours)
            .Must(BeValidExpiryHours)
            .WithMessage("Expiry hours must be between 1 and 168 (7 days)");
    }

    private bool BeValidFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return true;

        var invalidChars = Path.GetInvalidPathChars();
        return !filePath.Any(c => invalidChars.Contains(c));
    }

    private bool BeValidExpiryHours(int expiryHours)
    {
        return expiryHours > 0 && expiryHours <= 168;
    }

}