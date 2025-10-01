using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;

public class CreateUserCommandHandler(IUserService userService, IMapper mapper)
    : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var users = await userService.CreateUserAsync(request.User);

        return mapper.Map<UserDto>(users);
    }
}