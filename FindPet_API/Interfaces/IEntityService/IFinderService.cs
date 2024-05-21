using Models.DTO.FinderDTO;
using Models.Entities;

namespace Interfaces.IEntityService;

public interface IFinderService
{
    Task<IEnumerable<Finder>> GetFindersAsync();

    Task<Finder?> GetFinderAsync(Guid finderId);

    //Task<IEnumerable<Ad>?> GetAdsByFinderAsync(Guid finderId);

    //Task<IEnumerable<Pet>?> GetPetsByFinderAsync(Guid finderId);

    Task<bool> FinderExistsAsync(Guid finderId);

    Task<bool> FinderExistsAsync(string finderFirstName);

    Task DeleteFinderAsync(Guid finderId);

    Task UpdateFinderAsync(Guid finderId, FinderForUpdateDto finder);

    Task<Finder> CreateFinderAsync(FinderForCreateDto finder);
}