using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.BlobStorage;

public class CheckFileExistsQueryValidator : AbstractValidator<CheckFileExistsQuery>
{
    public CheckFileExistsQueryValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("File Path is required");
    }
}