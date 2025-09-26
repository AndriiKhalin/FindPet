using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Pet;

public class UpdatePetCommandValidator : AbstractValidator<UpdatePetCommand>
{
    public UpdatePetCommandValidator()
    {
        RuleFor(x => x.PetId)
            .GreaterThan(Guid.Empty)
            .WithMessage("Valid pet ID is required");

        RuleFor(x => x.PetUpdate.Nickname)
            .NotEmpty()
            .WithMessage("Pet name is required")
            .MaximumLength(100)
            .WithMessage("Pet name cannot exceed 100 characters");

        RuleFor(x => x.PetUpdate.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.PetUpdate.Breed)
            .NotEmpty()
            .WithMessage("Pet type is required")
            .Must(BeValidPetType)
            .WithMessage("Pet type must be Dog, Cat, Bird, or Other");

        RuleFor(x => x.PetUpdate.Breed)
            .NotEmpty()
            .WithMessage("Breed is required")
            .MaximumLength(50)
            .WithMessage("Breed cannot exceed 50 characters");

        RuleFor(x => x.PetUpdate.Color)
            .NotEmpty()
            .WithMessage("Color is required")
            .MaximumLength(50)
            .WithMessage("Color cannot exceed 50 characters");

        RuleFor(x => x.PetUpdate.Size)
            .NotEmpty()
            .WithMessage("Size is required")
            .Must(BeValidSize)
            .WithMessage("Size must be Small, Medium, Large, or Extra Large");

        RuleFor(x => x.PetUpdate.Gender)
            .NotEmpty()
            .WithMessage("Gender is required")
            .Must(BeValidGender)
            .WithMessage("Gender must be Male, Female, or Unknown");

        RuleFor(x => x.PetUpdate.LostDate)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Last seen date cannot be in the future");
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