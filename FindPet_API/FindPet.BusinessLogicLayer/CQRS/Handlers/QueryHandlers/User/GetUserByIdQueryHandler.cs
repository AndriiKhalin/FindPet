using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.User;
using FindPet.BusinessLogicLayer.Extensions;
using FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.User;

public class GetUserByIdQueryHandler(
    IUserService userService,
    IMapper mapper,
    IPhotoUrlTransformerService photoTransformer)
    : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(request.UserId);

        return await mapper.MapUserWithPhotoAsync(user!, photoTransformer);
    }
}