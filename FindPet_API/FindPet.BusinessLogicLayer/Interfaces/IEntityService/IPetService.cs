using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Interfaces.IEntityService;

public interface IPetService
{
    Task<IEnumerable<Pet>> GetPetsAsync();

    Task<Pet?> GetPetByIdAsync(Guid petId);

    //Task<IEnumerable<Ad>?> GetAdsByPetAsync(Guid petId);

    //Task<Finder> GetFinderByPetAsync(Guid petId);

    //Task<Owner> GetOwnerByPetAsync(Guid petId);

    Task<bool> PetExistsAsync(Guid petId);

    Task<bool> PetExistsAsync(string petName);

    Task DeletePetAsync(Guid petId);

    Task UpdatePetAsync(Guid petId, PetForUpdateDto pet);

    Task<Pet> CreatePetAsync(Guid userId, PetForCreateDto pet);
}