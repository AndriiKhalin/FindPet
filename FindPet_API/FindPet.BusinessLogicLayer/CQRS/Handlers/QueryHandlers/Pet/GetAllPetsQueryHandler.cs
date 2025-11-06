using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Pet;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Pet;

public class GetAllPetsQueryHandler(IPetService petService, IMapper mapper)
    : IQueryHandler<GetAllPetsQuery, IEnumerable<PetDto>>
{
    public async Task<IEnumerable<PetDto>> Handle(GetAllPetsQuery request, CancellationToken cancellationToken)
    {
        var pets = await petService.GetPetsAsync();

        return mapper.Map<IEnumerable<PetDto>>(pets);
    }
}