using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Ad;

public class CreateAdCommandHandler(IAdService adService, IMapper mapper) : ICommandHandler<CreateAdCommand, AdDto>
{
    public async Task<AdDto> Handle(CreateAdCommand request, CancellationToken cancellationToken)
    {
        var ad = await adService.CreateAdAsync(request.PetId, request.UserId, request.Ad);

        return mapper.Map<AdDto>(ad);
    }
}