using FindPet.BusinessLogicLayer.Interfaces.IPetMatchingService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Interfaces.ILoggerService;

namespace FindPet.BusinessLogicLayer.Services.PetMatchingService;

/// <summary>
/// Service for finding potential pet matches based on characteristics.
/// </summary>
public class PetMatchingService(IUnitOfWork unitOfWork, ILoggerManager logger) : IPetMatchingService
{
    public async Task<IEnumerable<Pet>> FindMatchesAsync(Pet newPet)
    {
        try
        {
            if (newPet == null)
            {
                logger.LogWarn("FindMatchesAsync called with null pet");
                return [];
            }

            // Determine opposite statuses for filtering
            var targetStatuses = GetOppositeStatuses(newPet.Status);

            // Find pets that could be a match based on:
            // - Same type (Dog, Cat, etc.) predicted by ML
            // - Same or similar breed
            // - Opposite status (if new pet is "Found", look for "Missing" pets)
            var potentialMatches = await unitOfWork.Pet.GetByConditionAsync(existingPet =>
                existingPet.Id != newPet.Id &&
                existingPet.UserId != newPet.UserId &&
                existingPet.UserId != null &&
                !string.IsNullOrEmpty(existingPet.Type) &&
                !string.IsNullOrEmpty(newPet.Type) &&
                existingPet.Type.ToLower() == newPet.Type.ToLower() &&
                !string.IsNullOrEmpty(existingPet.Status) &&
                targetStatuses.Contains(existingPet.Status.ToLower())
            );

            // Apply additional in-memory filtering for complex matching logic
            var refinedMatches = potentialMatches.Where(existingPet =>
                IsBreedMatch(existingPet.Breed, newPet.Breed) ||
                IsColorMatch(existingPet.Color, newPet.Color)
            ).ToList();

            logger.LogInfo($"Found {refinedMatches.Count} potential matches for pet type: {newPet.Type}");

            return refinedMatches;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error finding pet matches: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets the opposite status values for matching.
    /// Missing/Lost pets should match with Found/Sighted pets and vice versa.
    /// </summary>
    private static List<string> GetOppositeStatuses(string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            // If status unknown, return all possible statuses
            return new List<string> { "missing", "lost", "found", "sighted" };
        }

        var missingStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "missing", "lost" };
        var foundStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "found", "sighted" };

        if (missingStatuses.Contains(status))
        {
            return new List<string> { "found", "sighted" };
        }

        if (foundStatuses.Contains(status))
        {
            return new List<string> { "missing", "lost" };
        }

        // Unknown status - return all
        return new List<string> { "missing", "lost", "found", "sighted" };
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