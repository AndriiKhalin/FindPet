using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Interfaces.IPetMatchingService;

// <summary>
/// Service interface for finding potential pet matches based on ML predictions.
/// </summary>
public interface IPetMatchingService
{
    /// <summary>
    /// Finds potential matches for a newly created pet based on type, breed, and other characteristics.
    /// </summary>
    /// <param name="newPet">The newly created pet.</param>
    /// <returns>List of potentially matching pets.</returns>
    Task<IEnumerable<Pet>> FindMatchesAsync(Pet newPet);
}