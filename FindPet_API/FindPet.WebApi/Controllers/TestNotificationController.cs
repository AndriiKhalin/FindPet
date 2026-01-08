using FindPet.WebApi.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FindPet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TestNotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public TestNotificationController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Send a broadcast notification to all connected SignalR clients.
        /// </summary>
        /// <param name="message">The notification message</param>
        /// <returns>Success status</returns>
        [HttpPost("broadcast")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BroadcastNotification([FromBody] NotificationRequest request)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                Source = "TestController"
            });

            return Ok(new
            {
                success = true,
                message = "Broadcast sent to all connected clients",
                sentMessage = request.Message,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send a pet match notification to all connected clients.
        /// </summary>
        [HttpPost("pet-match")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendPetMatchNotification([FromBody] MatchNotificationRequest request)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMatchNotification", new
            {
                PetId = request.PetId,
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                NotificationType = "PetMatch"
            });

            return Ok(new
            {
                success = true,
                message = "Pet match notification sent",
                request.PetId,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get SignalR hub status.
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                hubEndpoint = "/notificationHub",
                status = "Active",
                timestamp = DateTime.UtcNow,
                testEndpoints = new[]
                {
                "POST /api/TestNotification/broadcast?message=Hello",
                "POST /api/TestNotification/pet-match?petId=123&message=Found!"
            }
            });
        }
    }

    public record NotificationRequest(string Message);
    public record MatchNotificationRequest(string PetId, string Message);
}
