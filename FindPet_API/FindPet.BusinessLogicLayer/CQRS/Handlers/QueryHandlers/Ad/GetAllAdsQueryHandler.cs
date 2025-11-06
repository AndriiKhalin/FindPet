using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Ad;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Ad;

public class GetAllAdsQueryHandler(IAdService adService, IMapper mapper)
    : IQueryHandler<GetAllAdsQuery, IEnumerable<AdDto>>
{
    public async Task<IEnumerable<AdDto>> Handle(GetAllAdsQuery request, CancellationToken cancellationToken)
    {
        var ads = await adService.GetAdsAsync();

        return mapper.Map<IEnumerable<AdDto>>(ads);
    }
}