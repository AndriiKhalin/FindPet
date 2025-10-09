using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class UploadMultipleFilesCommandValidator : AbstractValidator<UploadMultipleFilesCommand>
{
    private const int MaxFilesCount = 10;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB per file
    private const long MaxTotalSize = 50 * 1024 * 1024; // 50MB total
    private readonly string[] _allowedImageTypes =
        { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };

    public UploadMultipleFilesCommandValidator()
    {
        RuleFor(x => x.Files)
            .NotNull()
            .WithMessage("Files list is required")
            .NotEmpty()
            .WithMessage("At least one file is required")
            .Must(files => files.Count <= MaxFilesCount)
            .WithMessage($"Cannot upload more than {MaxFilesCount} files at once")
            .Must(HaveValidTotalSize)
            .WithMessage($"Total file size cannot exceed {MaxTotalSize / (1024 * 1024)}MB");

        RuleForEach(x => x.Files)
            .NotNull()
            .WithMessage("File cannot be null")
            .Must(file => file.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage($"Individual file size cannot exceed {MaxFileSize / (1024 * 1024)}MB")
            .Must(file => _allowedImageTypes.Contains(file.ContentType.ToLower()))
            .WithMessage("All files must be valid image types (JPEG, PNG, GIF, BMP, or WebP)")
            .Must(file => BeValidFileName(file.FileName))
            .WithMessage("File name contains invalid characters");

        RuleFor(x => x.Subfolder)
            .MaximumLength(100)
            .WithMessage("Subfolder name cannot exceed 100 characters")
            .Must(BeValidSubfolderName)
            .When(x => !string.IsNullOrEmpty(x.Subfolder))
            .WithMessage("Subfolder name contains invalid characters");
    }

    private bool HaveValidTotalSize(List<Microsoft.AspNetCore.Http.IFormFile> files)
    {
        return files.Sum(f => f.Length) <= MaxTotalSize;
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