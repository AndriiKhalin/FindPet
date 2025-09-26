using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Exceptions;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Pet;

public class UpdatePetCommandHandler(IPetService petService, IMapper mapper)
    : ICommandHandler<UpdatePetCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        await petService.UpdatePetAsync(request.PetId, request.PetUpdate);

        return Unit.Value;
    }
}