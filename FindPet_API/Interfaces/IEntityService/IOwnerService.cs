using Models.DTO.FinderDTO;
using Models.DTO.OwnerDTO;
using Models.Entities;

namespace Interfaces.IEntityService;

public interface IOwnerService
{
    Task<IEnumerable<Owner>> GetOwnersAsync();

    Task<Owner?> GetOwnerAsync(Guid ownerId);

    //Task<IEnumerable<Ad>?> GetAdsByOwnerAsync(Guid ownerId);

    //Task<IEnumerable<Pet>?> GetPetsByOwnerAsync(Guid ownerId);

    Task<bool> OwnerExistsAsync(Guid ownerId);

    Task<bool> OwnerExistsAsync(string ownerFirstName);

    Task DeleteOwnerAsync(Guid ownerId);

    Task UpdateOwnerAsync(Guid ownerId, OwnerForUpdateDto owner);

    Task<Owner> CreateOwnerAsync(OwnerForCreateDto owner);
}