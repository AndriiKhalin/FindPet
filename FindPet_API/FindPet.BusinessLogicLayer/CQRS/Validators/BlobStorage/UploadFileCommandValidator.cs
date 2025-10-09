using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedImageTypes =
        { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        When(x => x.File != null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File cannot be empty")
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage($"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB");

            RuleFor(x => x.File.ContentType)
                .Must(BeValidImageType)
                .WithMessage("File must be a valid image type (JPEG, PNG, GIF, BMP, or WebP)");

            RuleFor(x => x.File.FileName)
                .NotEmpty()
                .WithMessage("File name is required")
                .Must(BeValidFileName)
                .WithMessage("File name contains invalid characters");
        });

        RuleFor(x => x.Subfolder)
            .MaximumLength(100)
            .WithMessage("Subfolder name cannot exceed 100 characters")
            .Must(BeValidSubfolderName)
            .When(x => !string.IsNullOrEmpty(x.Subfolder))
            .WithMessage("Subfolder name contains invalid characters");
    }

    private bool BeValidImageType(string contentType)
    {
        return _allowedImageTypes.Contains(contentType.ToLower());
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