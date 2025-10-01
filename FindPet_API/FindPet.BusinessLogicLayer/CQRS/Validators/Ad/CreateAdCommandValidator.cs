using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Ad;

public class CreateAdCommandValidator : AbstractValidator<CreateAdCommand>
{
    public CreateAdCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.PetId)
            .NotEmpty()
            .WithMessage("PetId is required");

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