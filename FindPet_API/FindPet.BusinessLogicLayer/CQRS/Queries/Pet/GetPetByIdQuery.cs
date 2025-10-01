using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.Pet;

public record GetPetByIdQuery(Guid PetId) : IQuery<PetDto>;