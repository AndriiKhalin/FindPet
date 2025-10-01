using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Validators.Role;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public DeleteRoleCommandValidator(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required")
            .MustAsync(RoleExists)
            .WithMessage("Role not found");
    }

    private async Task<bool> RoleExists(string roleId, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        return role != null;
    }
}