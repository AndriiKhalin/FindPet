using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Account;

public class GetAllAuthUsersQueryHandler : IQueryHandler<GetAllAuthUsersQuery, IEnumerable<UserDetailDto>>
{
    private readonly UserManager<AuthUser> _userManager;

    public GetAllAuthUsersQueryHandler(UserManager<AuthUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<UserDetailDto>> Handle(GetAllAuthUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var userDtos = new List<UserDetailDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = roles.ToArray(),
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Photo = user.Photo,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            });
        }

        return userDtos;
    }
}