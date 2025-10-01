using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Role;

public class AssignRoleCommandHandler(RoleManager<IdentityRole> roleManager, UserManager<AuthUser> userManager)
    : ICommandHandler<AssignRoleCommand, IdentityResult>
{
    public async Task<IdentityResult> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.RoleAssignDto.UserId);
        var role = await roleManager.FindByIdAsync(request.RoleAssignDto.RoleId);

        return await userManager.AddToRoleAsync(user!, role!.Name!);
    }
}