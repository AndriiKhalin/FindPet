using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Ad;

public class UpdateAdCommandHandler(IAdService adService) : ICommandHandler<UpdateAdCommand, Unit>
{
    public async Task<Unit> Handle(UpdateAdCommand request, CancellationToken cancellationToken)
    {
        await adService.UpdateAdAsync(request.AdId, request.Ad);

        return Unit.Value;
    }
}