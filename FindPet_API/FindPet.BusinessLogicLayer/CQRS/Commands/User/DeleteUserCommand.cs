using FindPet.BusinessLogicLayer.CQRS.Common;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.User;

public record DeleteUserCommand(Guid UserId) : ICommand<Unit>;