using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Role;

public record CreateRoleCommand(CreateRoleDto CreateRoleDto) : ICommand<IdentityResult>;