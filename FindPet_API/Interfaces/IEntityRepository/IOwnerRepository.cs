using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IOwnerRepository
{
    Task<IEnumerable<Owner>> GetOwnersAsync();

    Task<Owner?> GetOwnerAsync(Guid ownerId);

    Task<IEnumerable<Ad>?> GetAdsByOwner(Guid ownerId);

    Task<IEnumerable<Pet>?> GetPetsByOwner(Guid ownerId);

    Task<bool> OwnerExistsAsync(Guid ownerId);

    Task<bool> OwnerExistsAsync(string ownerFirstName);

    Task DeleteOwnerAsync(Guid ownerId);

    Task UpdateOwnerAsync(Owner owner);

    Task CreateOwnerAsync(Owner owner);
}