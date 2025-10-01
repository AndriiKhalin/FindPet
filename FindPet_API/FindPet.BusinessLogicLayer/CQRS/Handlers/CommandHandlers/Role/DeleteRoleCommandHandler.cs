using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Role;

public class DeleteRoleCommandHandler(RoleManager<IdentityRole> roleManager) : ICommandHandler<DeleteRoleCommand, IdentityResult>
{
    public async Task<IdentityResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId);

        return await roleManager.DeleteAsync(role!);
    }
}