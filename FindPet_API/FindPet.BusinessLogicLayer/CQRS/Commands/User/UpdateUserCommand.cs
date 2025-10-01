using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.User;

public record UpdateUserCommand(Guid UserId, UserForUpdateDto User) : ICommand<Unit>;