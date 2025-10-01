using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FindPet.BusinessLogicLayer.CQRS.Common;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Role;

public class CreateRoleCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<CreateRoleCommand, IdentityResult>
{
    public async Task<IdentityResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        return await roleManager.CreateAsync(new IdentityRole(request.CreateRoleDto.RoleName));
    }
}