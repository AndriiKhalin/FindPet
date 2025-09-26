using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Pet;

public class DeletePetCommandHandler(IPetService petService) : ICommandHandler<DeletePetCommand, Unit>
{
    public async Task<Unit> Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        await petService.DeletePetAsync(request.PetId);
        return Unit.Value;
    }
}