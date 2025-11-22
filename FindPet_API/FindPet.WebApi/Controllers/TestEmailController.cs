using FindPet.Email.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestEmailController(IEmailService emailService) : ControllerBase
{
    /// <summary>
    ///     Test email sending
    /// </summary>
    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string email)
    {
        try
        {
            await emailService.SendWelcomeEmailAsync(email, "Test User");
            return Ok(new { message = "Email sent successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}