using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.User;

public record CreateUserCommand(UserForCreateDto User) : ICommand<UserDto>;