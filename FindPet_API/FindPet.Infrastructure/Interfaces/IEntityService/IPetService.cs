using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Interfaces.IEntityService;

public interface IPetService
{
    IEnumerable<Pet> GetPets();

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