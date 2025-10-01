using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Ad;

public class DeleteAdCommandHandler(IAdService adService) : ICommandHandler<DeleteAdCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAdCommand request, CancellationToken cancellationToken)
    {
        await adService
            .DeleteAdAsync(request.AdId);

        return Unit.Value;
    }
}