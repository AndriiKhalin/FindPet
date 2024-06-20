using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Interfaces.IEntityService;

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