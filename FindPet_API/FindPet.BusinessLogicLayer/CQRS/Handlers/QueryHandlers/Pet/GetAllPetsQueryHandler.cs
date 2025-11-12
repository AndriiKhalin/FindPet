using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Pet;
using FindPet.BusinessLogicLayer.Extensions;
using FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Pet;

public class GetAllPetsQueryHandler(
    IPetService petService,
    IMapper mapper,
    IPhotoUrlTransformerService photoTransformer)
    : IQueryHandler<GetAllPetsQuery, IEnumerable<PetDto>>
{
    public async Task<IEnumerable<PetDto>> Handle(GetAllPetsQuery request, CancellationToken cancellationToken)
    {
        var pets = await petService.GetPetsAsync();

        return await mapper.MapPetWithPhotosAsync(pets, photoTransformer);
    }
}