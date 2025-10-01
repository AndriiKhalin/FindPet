using FindPet.BusinessLogicLayer.CQRS.Common;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Pet;

public record DeletePetCommand(Guid PetId) : ICommand<Unit>;