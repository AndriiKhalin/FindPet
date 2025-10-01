using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Ad;

public record UpdateAdCommand(Guid AdId, AdForUpdateDto Ad) : ICommand<Unit>;