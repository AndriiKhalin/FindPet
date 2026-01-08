using FindPet.BusinessLogicLayer.Interfaces.INotificationService;
using FindPet.Domain.Interfaces.ILoggerService;
using Microsoft.AspNetCore.SignalR;

namespace FindPet.BusinessLogicLayer.Services.NotificationService;

/// <summary>
/// Implementation of notification service using SignalR for real-time communication.
/// </summary>
public class NotificationService<THub> : INotificationService where THub : Hub
{
    private readonly IHubContext<THub> _hubContext;
    private readonly ILoggerManager _logger;

    public NotificationService(IHubContext<THub> hubContext, ILoggerManager logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToAllAsync(string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Broadcast notification sent: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send broadcast notification: {ex.Message}");
        }
    }

    public async Task SendMatchNotificationAsync(Guid matchedPetId, Guid newPetId, string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMatchNotification", new
            {
                MatchedPetId = matchedPetId,
                NewPetId = newPetId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                NotificationType = "PetMatch"
            });

            _logger.LogInfo($"Match notification sent for pets {matchedPetId} and {newPetId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send match notification: {ex.Message}");
        }
    }

    public async Task SendToUserAsync(string userId, string message)
    {
        try
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Notification sent to user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send notification to user {userId}: {ex.Message}");
        }
    }

    public async Task SendToGroupAsync(string groupName, string message)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow,
                Group = groupName
            });

            _logger.LogInfo($"Notification sent to group {groupName}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send notification to group {groupName}: {ex.Message}");
        }
    }

    public async Task NotifyPetOwnerAsync(string ownerId, string lostPetName, Guid foundPetId)
    {
        try
        {
            var message = $"Great news! A pet matching '{lostPetName}' was just posted. Check it out!";

            await _hubContext.Clients.User(ownerId).SendAsync("ReceiveMatchNotification", new
            {
                Message = message,
                FoundPetId = foundPetId,
                LostPetName = lostPetName,
                Timestamp = DateTime.UtcNow,
                NotificationType = "PotentialMatch"
            });

            // Also send to user's personal group for redundancy
            await _hubContext.Clients.Group($"user_{ownerId}").SendAsync("ReceiveMatchNotification", new
            {
                Message = message,
                FoundPetId = foundPetId,
                LostPetName = lostPetName,
                Timestamp = DateTime.UtcNow,
                NotificationType = "PotentialMatch"
            });

            _logger.LogInfo($"Pet owner {ownerId} notified about potential match for '{lostPetName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to notify pet owner {ownerId}: {ex.Message}");
        }
    }
}