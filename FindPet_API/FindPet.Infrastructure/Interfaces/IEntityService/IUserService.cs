using FindPet.Domain.DTOs.EntitiesDTOs.OwnerDTO;
using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Interfaces.IEntityService;

public interface IUserService
{
    IEnumerable<Owner> GetUsers();

    Task<Owner?> GetOwnerAsync(Guid ownerId);

    Task<bool> OwnerExistsAsync(Guid ownerId);

    Task<bool> OwnerExistsAsync(string ownerFirstName);

    Task DeleteOwnerAsync(Guid ownerId);

    Task UpdateOwnerAsync(Guid ownerId, OwnerForUpdateDto owner);

    Task<Owner> CreateOwnerAsync(OwnerForCreateDto owner);
}