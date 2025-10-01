using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FluentValidation;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Account;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginDto)
            .NotNull()
            .WithMessage("Login data is required");

        When(x => x.LoginDto != null, () =>
        {
            RuleFor(x => x.LoginDto.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(256)
                .WithMessage("Email cannot exceed 256 characters");

            RuleFor(x => x.LoginDto.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters");
        });
    }
}