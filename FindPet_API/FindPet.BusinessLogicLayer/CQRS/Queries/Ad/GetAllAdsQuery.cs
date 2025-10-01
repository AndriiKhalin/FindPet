using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.Ad;

public record GetAllAdsQuery : IQuery<IEnumerable<AdDto>>;