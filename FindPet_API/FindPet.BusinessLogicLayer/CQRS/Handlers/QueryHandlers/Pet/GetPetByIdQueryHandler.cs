using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Pet;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Pet;

public class GetPetByIdQueryHandler(IPetService petService, IMapper mapper) : IQueryHandler<GetPetByIdQuery, PetDto?>
{
    public async Task<PetDto?> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        var pet = await petService.GetPetByIdAsync(request.PetId);

        return pet != null ? mapper.Map<PetDto>(pet) : null;
    }
}