using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Pet;
using FindPet.BusinessLogicLayer.Extensions;
using FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Exceptions;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Pet;

public class GetPetByIdQueryHandler(
    IPetService petService,
    IMapper mapper,
    IPhotoUrlTransformerService photoTransformer) : IQueryHandler<GetPetByIdQuery, PetDto?>
{
    public async Task<PetDto?> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        var pet = await petService.GetPetByIdAsync(request.PetId);

        if (pet == null)
            throw new NotFoundException("Pet", request.PetId);

        return await mapper.MapPetWithPhotoAsync(pet, photoTransformer);
    }
}