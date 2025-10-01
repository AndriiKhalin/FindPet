using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Pet;

public class CreatePetCommandHandler(IPetService petService, IMapper mapper) : ICommandHandler<CreatePetCommand, PetDto>
{
    public async Task<PetDto> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petService.CreatePetAsync(request.UserId, request.Pet);
        return mapper.Map<PetDto>(pet);
    }
}