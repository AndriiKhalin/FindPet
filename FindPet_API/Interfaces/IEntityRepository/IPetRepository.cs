using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Interfaces.IEntityRepository;

public interface IPetRepository
{
    Task<IEnumerable<Pet>> GetPetsAsync();

    Task<Pet?> GetPetAsync(Guid petId);

    Task<IEnumerable<Ad>?> GetAdsByPet(Guid petId);

    Task<Finder> GetFinderByPet(Guid petId);

    Task<Owner> GetOwnerByPet(Guid petId);

    Task<bool> PetExistsAsync(Guid petId);

    Task<bool> PetExistsAsync(string petName);

    Task DeletePetAsync(Guid petId);

    Task UpdatePetAsync(Pet pet);

    Task CreatePetAsync(Pet pet);
}