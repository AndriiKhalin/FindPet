using Models.Entities;
using System.Security.Cryptography.Xml;
using Models.DTO.PetDTO;

namespace Interfaces.IEntityService;

public interface IPetService
{
    Task<IEnumerable<Pet>> GetPetsAsync();

    Task<Pet?> GetPetAsync(Guid petId);

    //Task<IEnumerable<Ad>?> GetAdsByPetAsync(Guid petId);

    //Task<Finder> GetFinderByPetAsync(Guid petId);

    //Task<Owner> GetOwnerByPetAsync(Guid petId);

    Task<bool> PetExistsAsync(Guid petId);

    Task<bool> PetExistsAsync(string petName);

    Task DeletePetAsync(Guid petId);

    Task UpdatePetAsync(Guid petId, PetForUpdateDto pet);

    Task<Pet> CreatePetAsync(Guid ownerId, Guid finderId, PetForCreateDto pet);
}