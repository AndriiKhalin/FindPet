using Models.DTO.AdDTO;
using Models.DTO.FinderDTO;
using Models.Entities;

namespace Interfaces.IEntityService;

public interface IAdService
{
    IEnumerable<Ad> GetAds();

    Task<Ad?> GetAdAsync(Guid adId);

    //Task<Pet?> GetPetByAd(Guid adId);

    //Task<User> GetUserByAd(Guid adId);

    Task<bool> AdExistsAsync(Guid adId);

    Task DeleteAdAsync(Guid adId);

    Task UpdateAdAsync(Guid adId, AdForUpdateDto ad);

    Task<Ad> CreateAdAsync(Guid petId, Guid userId, AdForCreateDto ad);
}