using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IAdRepository
{
    Task<IEnumerable<Ad>> GetAdsAsync();

    Task<Ad?> GetAdAsync(Guid adId);

    Task<Pet?> GetPetByAd(Guid adId);

    Task<IUser> GetUserByAd(Guid adId);

    Task<bool> AdExistsAsync(Guid adId);

    Task DeleteAdAsync(Guid adId);

    Task UpdateAdAsync(Ad ad);

    Task CreateAdAsync(Ad ad);
}