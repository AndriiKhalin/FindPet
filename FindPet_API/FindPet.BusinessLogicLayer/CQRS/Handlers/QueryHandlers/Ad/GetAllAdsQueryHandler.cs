using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Ad;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Ad;

public class GetAllAdsQueryHandler(IAdService adService, IMapper mapper)
    : IQueryHandler<GetAllAdsQuery, IEnumerable<AdDto>>
{
    public Task<IEnumerable<AdDto>> Handle(GetAllAdsQuery request, CancellationToken cancellationToken)
    {
        var ads = adService.GetAds();

        var adsDto = mapper.Map<IEnumerable<AdDto>>(ads);

        return Task.FromResult(adsDto);
    }
}