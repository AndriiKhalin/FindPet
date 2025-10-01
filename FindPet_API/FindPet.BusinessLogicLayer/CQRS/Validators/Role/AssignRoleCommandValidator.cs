using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FindPet.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Role;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AuthUser> _userManager;

    public AssignRoleCommandValidator(RoleManager<IdentityRole> roleManager, UserManager<AuthUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;

        RuleFor(x => x.RoleAssignDto)
            .NotNull()
            .WithMessage("Role assignment information is required");

        When(x => x.RoleAssignDto != null, () =>
        {
            RuleFor(x => x.RoleAssignDto.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .MustAsync(UserExists)
                .WithMessage("User not found");

            RuleFor(x => x.RoleAssignDto.RoleId)
                .NotEmpty()
                .WithMessage("Role ID is required")
                .MustAsync(RoleExists)
                .WithMessage("Role not found");

            RuleFor(x => x)
                .MustAsync(UserNotInRole)
                .WithMessage("User already has this role assigned");
        });
    }

    private async Task<bool> UserExists(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null;
    }

    private async Task<bool> RoleExists(string roleId, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        return role != null;
    }

    private async Task<bool> UserNotInRole(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.RoleAssignDto.UserId);
        var role = await _roleManager.FindByIdAsync(command.RoleAssignDto.RoleId);

        if (user == null || role == null)
            return true; // This will be caught by the other validators

        return !await _userManager.IsInRoleAsync(user, role.Name!);
    }
}