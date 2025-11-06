using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Interfaces.IEntityService;

public interface IAdService
{
    Task<IEnumerable<Ad>> GetAdsAsync();

    Task<Ad?> GetAdAsync(Guid adId);

    //Task<Pet?> GetPetByAd(Guid adId);

    //Task<User> GetUserByAd(Guid adId);

    Task<bool> AdExistsAsync(Guid adId);

    Task DeleteAdAsync(Guid adId);

    Task UpdateAdAsync(Guid adId, AdForUpdateDto ad);

    Task<Ad> CreateAdAsync(Guid petId, Guid userId, AdForCreateDto ad);
}