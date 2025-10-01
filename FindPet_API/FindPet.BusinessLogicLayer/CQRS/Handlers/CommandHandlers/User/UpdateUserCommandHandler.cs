using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;

public class UpdateUserCommandHandler(IUserService userService) : ICommandHandler<UpdateUserCommand, Unit>
{
    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        await userService.UpdateUserAsync(request.UserId, request.User);

        return Unit.Value;
    }
}