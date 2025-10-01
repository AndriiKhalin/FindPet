using FindPet.BusinessLogicLayer.CQRS.Commands.Image;
using FindPet.Domain.Enums;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Image;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private const int MaxDisplayedEntityTypes = 5;
    private readonly EntityType[] _allowedEntityTypes = { EntityType.User, EntityType.Pet };

    private readonly string[] _allowedImageTypes =
        { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.ImageFile)
            .NotNull().WithMessage("Image file is required");

        When(x => x.ImageFile != null, () =>
        {
            RuleFor(x => x.ImageFile.Length)
                .GreaterThan(0).WithMessage("File cannot be empty")
                .LessThanOrEqualTo(MaxFileSize).WithMessage($"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB");

            RuleFor(x => x.ImageFile.ContentType)
                .Must(BeValidImageType).WithMessage("File must be a valid image type (JPEG, PNG, GIF, BMP, or WebP)");

            RuleFor(x => x.ImageFile.FileName)
                .NotEmpty()
                .WithMessage("File name is required")
                .Must(BeValidFileName)
                .WithMessage("File name contains invalid characters");

            RuleFor(x => x.EntityType)
                .NotEmpty()
                .WithMessage("Entity type is required")
                .Must(BeValidEntityType)
                .WithMessage($"Entity type must be one of: {FormatAllowedEntityTypes()}");
        });
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

    private bool BeValidEntityType(EntityType entityType)
    {
        return _allowedEntityTypes.Contains(entityType);
    }

    private string FormatAllowedEntityTypes()
    {
        if (_allowedEntityTypes.Length <= MaxDisplayedEntityTypes) return string.Join(", ", _allowedEntityTypes);

        var displayedEntities = _allowedEntityTypes.Take(MaxDisplayedEntityTypes);
        return string.Join(", ", displayedEntities) + "...";
    }
}