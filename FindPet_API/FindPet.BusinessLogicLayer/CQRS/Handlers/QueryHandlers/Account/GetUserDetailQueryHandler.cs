using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Account;

public class GetUserDetailQueryHandler : IQueryHandler<GetUserDetailQuery, UserDetailDto>
{
    private readonly UserManager<AuthUser> _userManager;

    public GetUserDetailQueryHandler(UserManager<AuthUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user is null)
            throw new NotFoundException("User", request.UserId);

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Photo = user.Photo,
            BirthDate = user.BirthDate,
            Role = roles.ToArray(),
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            AccessFailedCount = user.AccessFailedCount
        };
    }
}