using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Ad;

public class UpdateAdCommandValidator : AbstractValidator<UpdateAdCommand>
{
    public UpdateAdCommandValidator()
    {
        RuleFor(x => x.AdId)
            .NotEmpty()
            .WithMessage("AdId is required");

        When(x => x.Ad != null, () =>
        {
            RuleFor(x => x.Ad.Description)
                .NotEmpty()
                .WithMessage("Description is required");

            RuleFor(x => x.Ad.Location)
                .NotEmpty()
                .WithMessage("Location is required");

            RuleFor(x => x.Ad.Photo)
                .NotEmpty()
                .WithMessage("Photo is required");
        });
    }
}