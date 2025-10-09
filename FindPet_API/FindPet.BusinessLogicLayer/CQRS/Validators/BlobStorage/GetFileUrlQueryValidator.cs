using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class GetFileUrlQueryValidator : AbstractValidator<GetFileUrlQuery>
{
    public GetFileUrlQueryValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(BeValidFileName)
            .WithMessage("File name contains invalid characters");

        RuleFor(x => x.Subfolder)
            .MaximumLength(100)
            .WithMessage("Subfolder name cannot exceed 100 characters")
            .Must(BeValidSubfolderName)
            .When(x => !string.IsNullOrEmpty(x.Subfolder))
            .WithMessage("Subfolder name contains invalid characters");
    }

    private bool BeValidFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        return !fileName.Any(c => invalidChars.Contains(c));
    }

    private bool BeValidSubfolderName(string subfolder)
    {
        if (string.IsNullOrEmpty(subfolder)) return true;

        var invalidChars = Path.GetInvalidPathChars().Concat(new[] { '/', '\\' });
        return !subfolder.Any(c => invalidChars.Contains(c));
    }
}