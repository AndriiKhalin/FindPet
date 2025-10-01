using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.User;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserService userService)
    {
        RuleFor(x => x.User)
            .NotNull()
            .WithMessage("User information is required");

        When(x => x.User != null, () =>
        {
            RuleFor(x => x.User.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters")
                .MinimumLength(2)
                .WithMessage("Name must be at least 2 characters long")
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("Name can only contain letters, spaces, hyphens and apostrophes");

            RuleFor(x => x.User.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(256)
                .WithMessage("Email cannot exceed 256 characters")
                .MustAsync(async (email, cancellation) => await userService.IsEmailRegisteredAsync(email))
                .WithMessage("Email address is already registered");

            RuleFor(x => x.User.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password must be maximum 100 characters")
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]+").WithMessage("Password must contain at least one number")
                .Matches(@"[\W_]+").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.User.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Invalid phone number format")
                .MustAsync(async (phoneNumber, cancellation) =>
                    await userService.IsPhoneNumberRegisteredAsync(phoneNumber))
                .WithMessage("Phone number is already registered");

            RuleFor(x => x.User.BirthDate)
                .NotNull()
                .WithMessage("Birth date is required")
                .LessThan(DateTime.UtcNow).WithMessage("Birth date cannot be in the future")
                .Must(BeValidAge)
                .WithMessage("User must be at least 13 years old and birth date cannot be in the future");

            RuleFor(x => x.User.Photo)
                .MaximumLength(500)
                .WithMessage("Photo path cannot exceed 500 characters")
                .Must(BeValidImagePath)
                .When(x => !string.IsNullOrEmpty(x.User.Photo))
                .WithMessage("Photo must be a valid image file (.jpg, .jpeg, .png, .gif)");
        });
    }

    private bool BeValidAge(DateTime? birthDate)
    {
        if (!birthDate.HasValue) return false;

        var today = DateTime.Today;
        var age = today.Year - birthDate.Value.Year;

        if (birthDate.Value.Date > today.AddYears(-age))
            age--;

        return age >= 13 && birthDate.Value <= today && age <= 128;
    }

    private bool BeValidImagePath(string photoPath)
    {
        if (string.IsNullOrEmpty(photoPath)) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(photoPath).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }
}