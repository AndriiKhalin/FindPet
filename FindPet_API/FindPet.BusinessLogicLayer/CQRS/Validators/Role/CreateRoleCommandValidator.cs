using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Role;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public CreateRoleCommandValidator(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;

        RuleFor(x => x.CreateRoleDto)
            .NotNull()
            .WithMessage("Role information is required");

        When(x => x.CreateRoleDto != null, () =>
        {
            RuleFor(x => x.CreateRoleDto.RoleName)
                .NotEmpty()
                .WithMessage("Role name is required")
                .MaximumLength(256)
                .WithMessage("Role name cannot exceed 256 characters")
                .MinimumLength(2)
                .WithMessage("Role name must be at least 2 characters long")
                .Matches(@"^[a-zA-Z0-9_-]+$")
                .WithMessage("Role name can only contain letters, numbers, underscores and hyphens")
                .MustAsync(BeUniqueRoleName)
                .WithMessage("Role already exists");
        });
    }

    private async Task<bool> BeUniqueRoleName(string roleName, CancellationToken cancellationToken)
    {
        return !await _roleManager.RoleExistsAsync(roleName);
    }
}