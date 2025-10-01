using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Pet;

public record UpdatePetCommand(Guid PetId, PetForUpdateDto PetUpdate) : ICommand<Unit>;