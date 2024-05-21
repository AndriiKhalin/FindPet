using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IAdRepository : IBaseRepository<Ad>
{
    Task<IEnumerable<Ad>> GetsAsync();

    Task<Ad?> GetAsync(Guid adId);

    //Task<Pet?> GetPetByAd(Guid adId);

    //Task<User> GetUserByAd(Guid adId);

    Task<bool> IsExistAsync(Guid adId);

    Task DeleteAsync(Guid adId);

    Task UpdateAsync(Ad ad);

    Task CreateAsync(Ad ad);
}