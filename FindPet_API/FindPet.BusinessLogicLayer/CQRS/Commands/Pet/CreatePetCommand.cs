using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Pet;

public record CreatePetCommand(Guid UserId, PetForCreateDto PetCreate) : ICommand<PetDto>;