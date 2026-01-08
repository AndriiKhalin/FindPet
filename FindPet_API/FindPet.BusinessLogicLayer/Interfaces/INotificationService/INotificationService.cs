namespace FindPet.BusinessLogicLayer.Interfaces.INotificationService;


/// <summary>
/// Service interface for sending real-time notifications via SignalR.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="message">The notification message.</param>
    Task SendToAllAsync(string message);

    /// <summary>
    /// Sends a pet match notification to all connected clients.
    /// </summary>
    /// <param name="matchedPetId">The ID of the matched pet.</param>
    /// <param name="newPetId">The ID of the newly created pet.</param>
    /// <param name="message">The notification message.</param>
    Task SendMatchNotificationAsync(Guid matchedPetId, Guid newPetId, string message);

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="message">The notification message.</param>
    Task SendToUserAsync(string userId, string message);

    /// <summary>
    /// Sends a notification to a group of users (e.g., by pet type or location).
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="message">The notification message.</param>
    Task SendToGroupAsync(string groupName, string message);

    /// <summary>
    /// Notifies the owner of a lost pet about a potential match.
    /// </summary>
    /// <param name="ownerId">The owner's user ID.</param>
    /// <param name="lostPetName">The name of the lost pet.</param>
    /// <param name="foundPetId">The ID of the potentially matching found pet.</param>
    Task NotifyPetOwnerAsync(string ownerId, string lostPetName, Guid foundPetId);
}