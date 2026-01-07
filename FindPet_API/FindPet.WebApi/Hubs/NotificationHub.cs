using FindPet.Domain.Interfaces.ILoggerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FindPet.WebApi.Hubs;

/// <summary>
/// SignalR hub for real-time notifications in the FindPet application.
/// Handles pet match notifications, status updates, and user-specific alerts.
/// </summary>
public class NotificationHub : Hub
{
    private readonly ILoggerManager _logger;
    public NotificationHub(ILoggerManager logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="message">The notification message.</param>
    [AllowAnonymous]
    public async Task SendNotification(string message)
    {
        _logger.LogInfo($"SendNotification called with message: {message}");
        await Clients.All.SendAsync("ReceiveNotification", message);
        _logger.LogInfo("ReceiveNotification sent to all clients");
    }

    /// <summary>
    /// Sends a pet match notification to all connected clients.
    /// </summary>
    /// <param name="matchedPetId">The ID of the matched pet.</param>
    /// <param name="message">The notification message.</param>
    [AllowAnonymous]
    public async Task SendMatchNotification(string matchedPetId, string message)
    {
        _logger.LogInfo($"SendMatchNotification called - PetId: {matchedPetId}, Message: {message}");
        await Clients.All.SendAsync("ReceiveMatchNotification", new
        {
            PetId = matchedPetId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="message">The notification message.</param>
    [AllowAnonymous]
    public async Task SendNotificationToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Joins a user to a specific notification group (e.g., by pet type or location).
    /// </summary>
    /// <param name="groupName">The name of the group to join.</param>
    [AllowAnonymous]
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Leaves a notification group.
    /// </summary>
    /// <param name="groupName">The name of the group to leave.</param>
    [AllowAnonymous]
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInfo($"Client connected: {Context.ConnectionId}");
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInfo($"Client disconnected: {Context.ConnectionId}, Exception: {exception?.Message}");

        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}