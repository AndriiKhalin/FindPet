using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Ad;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.Exceptions;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Ad;

public class GetAdByIdQueryHandler(IAdService adService, IMapper mapper) : IQueryHandler<GetAdByIdQuery, AdDto>
{
    public async Task<AdDto> Handle(GetAdByIdQuery request, CancellationToken cancellationToken)
    {
        var ad = await adService.GetAdAsync(request.AdId);

        if (ad == null)
            throw new NotFoundException("Ad", request.AdId);

        return mapper.Map<AdDto>(ad);
    }
}