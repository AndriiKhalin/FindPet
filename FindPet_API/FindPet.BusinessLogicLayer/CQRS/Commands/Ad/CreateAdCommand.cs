using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Ad;

public record CreateAdCommand(Guid PetId, Guid UserId, AdForCreateDto Ad) : ICommand<AdDto>;