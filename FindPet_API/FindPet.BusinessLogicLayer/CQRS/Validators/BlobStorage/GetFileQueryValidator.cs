using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class GetFileQueryValidator : AbstractValidator<GetFileQuery>
{
    public GetFileQueryValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("File Path is required");
    }
}