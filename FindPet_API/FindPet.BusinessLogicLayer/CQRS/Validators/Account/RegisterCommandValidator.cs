using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.Entities;
using FindPet.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Account;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly IUserService _userService;

    public RegisterCommandValidator(UserManager<AuthUser> userManager, IUserService userService)
    {
        _userManager = userManager;
        _userService = userService;

        RuleFor(x => x.RegisterDto)
            .NotNull()
            .WithMessage("Registration data is required");

        When(x => x.RegisterDto != null, () =>
        {
            RuleFor(x => x.RegisterDto.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters")
                .MinimumLength(2)
                .WithMessage("Name must be at least 2 characters long")
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("Name can only contain letters, spaces, hyphens and apostrophes");

            RuleFor(x => x.RegisterDto.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(256)
                .WithMessage("Email cannot exceed 256 characters")
                .MustAsync(async (email, _) => await _userManager.FindByEmailAsync(email) == null)
                .WithMessage("Email address is already registered")
                .MustAsync(async (email, _) => !await _userService.IsEmailRegisteredAsync(email))
                .WithMessage("Email is already registered");

            RuleFor(x => x.RegisterDto.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters")
                .Matches(@"[A-Z]+")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]+")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]+")
                .WithMessage("Password must contain at least one number")
                .Matches(@"[\W_]+")
                .WithMessage("Password must contain at least one special character");

            RuleFor(x => x.RegisterDto.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Invalid phone number format")
                .MustAsync(async (phoneNumber, _) =>
                    !await _userService.IsPhoneNumberRegisteredAsync(phoneNumber))
                .WithMessage("Phone number is already registered");

            RuleFor(x => x.RegisterDto.BirthDate)
                .NotNull()
                .WithMessage("Birth date is required")
                .LessThan(DateTime.UtcNow)
                .WithMessage("Birth date cannot be in the future")
                .Must(BeValidAge)
                .WithMessage("User must be at least 13 years old");

            RuleFor(x => x.RegisterDto.Role)
                .Must(BeValidRole)
                .When(x => !string.IsNullOrEmpty(x.RegisterDto.Role))
                .WithMessage("Invalid role specified");

            RuleFor(x => x.RegisterDto.Photo)
                .MaximumLength(500)
                .WithMessage("Photo path cannot exceed 500 characters")
                .Must(BeValidImagePath)
                .When(x => !string.IsNullOrEmpty(x.RegisterDto.Photo))
                .WithMessage("Photo must be a valid image file (.jpg, .jpeg, .png, .gif)");
        });
    }

    private bool BeValidAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age >= 13 && age <= 120;
    }

    private bool BeValidRole(string? role)
    {
        if (string.IsNullOrEmpty(role)) return true;

        var validRoles = new[] { UserRoles.Admin, UserRoles.User };
        return validRoles.Contains(role);
    }

    private bool BeValidImagePath(string? photoPath)
    {
        if (string.IsNullOrEmpty(photoPath)) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(photoPath).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }
}