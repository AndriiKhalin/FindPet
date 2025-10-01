using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.User;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.User;

public class GetAllUsersQueryHandler(IUserService userService, IMapper mapper)
    : IQueryHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = userService.GetUsers();

        var usersMap = mapper.Map<IEnumerable<UserDto>>(users);

        return Task.FromResult(usersMap);
    }
}