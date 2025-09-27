using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Pet;

public class CreatePetCommandValidator : AbstractValidator<CreatePetCommand>
{
    public CreatePetCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("Valid user ID is required");

        RuleFor(x => x.PetCreate)
            .NotNull()
            .WithMessage("Pet information is required");

        When(x => x.PetCreate != null, () =>
        {
            RuleFor(x => x.PetCreate.Nickname)
                .NotEmpty()
                .WithMessage("Pet name is required")
                .MaximumLength(100)
                .WithMessage("Pet name cannot exceed 100 characters");

            RuleFor(x => x.PetCreate.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.PetCreate.Breed)
                .NotEmpty()
                .WithMessage("Pet type is required")
                .Must(BeValidPetType)
                .WithMessage("Pet type must be Dog, Cat, Bird, or Other");

            RuleFor(x => x.PetCreate.Breed)
                .NotEmpty()
                .WithMessage("Breed is required")
                .MaximumLength(50)
                .WithMessage("Breed cannot exceed 50 characters");

            RuleFor(x => x.PetCreate.Color)
                .NotEmpty()
                .WithMessage("Color is required")
                .MaximumLength(50)
                .WithMessage("Color cannot exceed 50 characters");

            RuleFor(x => x.PetCreate.Size)
                .NotEmpty()
                .WithMessage("Size is required")
                .Must(BeValidSize)
                .WithMessage("Size must be Small, Medium, Large, or Extra Large");

            RuleFor(x => x.PetCreate.Gender)
                .NotEmpty()
                .WithMessage("Gender is required")
                .Must(BeValidGender)
                .WithMessage("Gender must be Male, Female, or Unknown");

            RuleFor(x => x.PetCreate.LostDate)
                .LessThanOrEqualTo(DateTime.Now)
                .When(x => x.PetCreate.LostDate.HasValue)
                .WithMessage("Lost date cannot be in the future");
        });
    }

    private bool BeValidPetType(string petType)
    {
        var validTypes = new[] { "Dog", "Cat", "Bird", "Other" };
        return validTypes.Contains(petType, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidSize(string size)
    {
        var validSizes = new[] { "Small", "Medium", "Large", "Extra Large" };
        return validSizes.Contains(size, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidGender(string gender)
    {
        var validGenders = new[] { "Male", "Female", "Unknown" };
        return validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase);
    }
}