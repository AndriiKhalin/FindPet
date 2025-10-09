using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileUrl)
            .NotEmpty()
            .WithMessage("File URL is required")
            .Must(BeValidUrl)
            .WithMessage("File URL must be a valid URL");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}