using FindPet.Domain.DTOs.FinderDTO;
using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Interfaces.IEntityService;

public interface IFinderService
{
    IEnumerable<Finder> GetFinders();

    Task<Finder?> GetFinderAsync(Guid finderId);

    //Task<IEnumerable<Ad>?> GetAdsByFinderAsync(Guid finderId);

    //Task<IEnumerable<Pet>?> GetPetsByFinderAsync(Guid finderId);

    Task<bool> FinderExistsAsync(Guid finderId);

    Task<bool> FinderExistsAsync(string finderFirstName);

    Task DeleteFinderAsync(Guid finderId);

    Task UpdateFinderAsync(Guid finderId, FinderForUpdateDto finder);

    Task<Finder> CreateFinderAsync(FinderForCreateDto finder);
}