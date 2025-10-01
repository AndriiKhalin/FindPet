using FindPet.BusinessLogicLayer.CQRS.Common;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Role;

public record DeleteRoleCommand(string RoleId) : ICommand<IdentityResult>;