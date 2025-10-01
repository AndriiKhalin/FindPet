using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Role;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Role;

public class GetAllRolesQueryHandler(RoleManager<IdentityRole> roleManager, UserManager<AuthUser> userManager)
    : IQueryHandler<GetAllRolesQuery, IEnumerable<RoleResponseDto>>
{
    public async Task<IEnumerable<RoleResponseDto>> Handle(GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles.ToListAsync();

        var roleDtos = new List<RoleResponseDto>();

        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            roleDtos.Add(new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                TotalUsers = usersInRole.Count
            });
        }

        return roleDtos;
    }
}