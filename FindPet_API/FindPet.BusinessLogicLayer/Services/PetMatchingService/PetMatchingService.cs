using FindPet.BusinessLogicLayer.Interfaces.IPetMatchingService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Interfaces.ILoggerService;

namespace FindPet.BusinessLogicLayer.Services.PetMatchingService;

/// <summary>
/// Service for finding potential pet matches based on characteristics.
/// </summary>
public class PetMatchingService : IPetMatchingService
{
    private readonly ILoggerManager _logger;
    private readonly IUnitOfWork _unitOfWork;

    public PetMatchingService(IUnitOfWork unitOfWork, ILoggerManager logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Pet>> FindMatchesAsync(Pet newPet)
    {
        try
        {
            var allPets = await _unitOfWork.Pet.GetsAsync();

            // Find pets that could be a match based on:
            // - Same type (Dog, Cat, etc.) predicted by ML
            // - Same or similar breed
            // - Opposite status (if new pet is "Found", look for "Missing" pets)
            var potentialMatches = allPets.Where(existingPet =>
                existingPet.Id != newPet.Id &&
                existingPet.UserId != newPet.UserId &&
                IsTypeMatch(existingPet.Type, newPet.Type) &&
                IsStatusOpposite(existingPet.Status, newPet.Status) &&
                (IsBreedMatch(existingPet.Breed, newPet.Breed) ||
                 IsColorMatch(existingPet.Color, newPet.Color))
            ).ToList();

            _logger.LogInfo($"Found {potentialMatches.Count} potential matches for pet type: {newPet.Type}");

            return potentialMatches;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error finding pet matches: {ex.Message}");
            return Enumerable.Empty<Pet>();
        }
    }

    private static bool IsTypeMatch(string? existingType, string? newType)
    {
        if (string.IsNullOrEmpty(existingType) || string.IsNullOrEmpty(newType))
            return false;

        return existingType.Equals(newType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBreedMatch(string? existingBreed, string? newBreed)
    {
        if (string.IsNullOrEmpty(existingBreed) || string.IsNullOrEmpty(newBreed))
            return false;

        return existingBreed.Equals(newBreed, StringComparison.OrdinalIgnoreCase) ||
               existingBreed.Contains(newBreed, StringComparison.OrdinalIgnoreCase) ||
               newBreed.Contains(existingBreed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsColorMatch(string? existingColor, string? newColor)
    {
        if (string.IsNullOrEmpty(existingColor) || string.IsNullOrEmpty(newColor))
            return false;

        return existingColor.Equals(newColor, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStatusOpposite(string? existingStatus, string? newStatus)
    {
        if (string.IsNullOrEmpty(existingStatus) || string.IsNullOrEmpty(newStatus))
            return true; // If status is unknown, consider it a potential match

        // "Missing" pets should match with "Found" pets
        var missingStatuses = new[] { "Missing", "Lost" };
        var foundStatuses = new[] { "Found", "Sighted" };

        var existingIsMissing = missingStatuses.Any(s =>
            existingStatus.Equals(s, StringComparison.OrdinalIgnoreCase));
        var newIsFound = foundStatuses.Any(s =>
            newStatus.Equals(s, StringComparison.OrdinalIgnoreCase));

        var existingIsFound = foundStatuses.Any(s =>
            existingStatus.Equals(s, StringComparison.OrdinalIgnoreCase));
        var newIsMissing = missingStatuses.Any(s =>
            newStatus.Equals(s, StringComparison.OrdinalIgnoreCase));

        return (existingIsMissing && newIsFound) || (existingIsFound && newIsMissing);
    }
}