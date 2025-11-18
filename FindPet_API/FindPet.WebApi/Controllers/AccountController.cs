using System.Security.Claims;
using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator, ITokenService tokenService) : ControllerBase
{
    private string RefreshTokenFromCookie => Request.Cookies["refreshToken"] ?? string.Empty;
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    #region Email Confirmation

    /// <summary>
    ///     Confirm email address with token
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Confirmation token from email</param>
    /// <returns>Confirmation result</returns>
    [AllowAnonymous]
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest(new { message = "User ID and token are required" });

        var response = await mediator.Send(new ConfirmEmailCommand(userId, token));

        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    #endregion

    #region Token Management

    /// <summary>
    ///     Refresh access token using refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        if (string.IsNullOrEmpty(RefreshTokenFromCookie))
            return Unauthorized("Refresh token is required in cookie");

        var response = await mediator.Send(new RefreshTokenCommand(RefreshTokenFromCookie));

        SetRefreshTokenCookie(response.RefreshToken!, response.RefreshTokenExpiration);

        return Ok(response);
    }

    //[Authorize]
    //[HttpPost("revoke-token")]
    //[ProducesResponseType(200)]
    //[ProducesResponseType(400)]
    //public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    //{
    //    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    //    var result = await _tokenService.RevokeTokenAsync(request.Token, ipAddress);

    //    if (!result)
    //        return BadRequest(new { message = "Invalid token" });

    //    return Ok(new { message = "Token revoked successfully" });
    //}

    #endregion

    #region Registration & Login

    /// <summary>
    ///     Register a new user
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await mediator.Send(new RegisterCommand(registerDto));
        return Ok(response);
    }

    /// <summary>
    ///     Login with email and password
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await mediator.Send(new LoginCommand(loginDto));

        SetRefreshTokenCookie(response.RefreshToken!, response.RefreshTokenExpiration);

        return Ok(response);
    }

    /// <summary>
    ///     Logout (revoke refresh token)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await mediator.Send(new LogoutCommand(RefreshTokenFromCookie));

        ClearRefreshTokenCookie();

        return Ok(new { message = "Logged out successfully" });
    }

    #endregion

    #region Password Management

    /// <summary>
    ///     Request password reset
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponse>> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await mediator.Send(new ForgotPasswordCommand(forgotPassword));
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Reset password with token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] ResetPasswordDto resetPassword)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await mediator.Send(new ResetPasswordCommand(resetPassword));

        if (!response.IsSuccess)
            return BadRequest(response);

        // Clear any existing refresh tokens
        ClearRefreshTokenCookie();

        return Ok(response);
    }

    /// <summary>
    ///     Change password for authenticated user
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordDto changePassword)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized(new { message = "User not authenticated" });

        var response = await mediator.Send(new ChangePasswordCommand(CurrentUserId, changePassword));

        if (!response.IsSuccess)
            return BadRequest(response);

        // Clear all refresh tokens (user will need to login again)
        ClearRefreshTokenCookie();

        return Ok(response);
    }

    #endregion

    #region User Information

    /// <summary>
    ///     Get current user details
    /// </summary>
    [Authorize]
    [HttpGet("detail")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDetailDto>> GetUserDetail()
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized(new { message = "User not authenticated" });

        try
        {
            var userDetail = await mediator.Send(new GetUserDetailQuery(CurrentUserId));
            return Ok(userDetail);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Get all users (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
    {
        var users = await mediator.Send(new GetAllAuthUsersQuery());
        return Ok(users);
    }

    #endregion

    #region Session Management

    /// <summary>
    ///     Get all active sessions for current user
    /// </summary>
    [Authorize]
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<SessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetActiveSessions()
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized(new { message = "User not authenticated" });

        var sessions = await mediator.Send(new GetActiveSessionsQuery(CurrentUserId, RefreshTokenFromCookie));
        return Ok(sessions);
    }

    /// <summary>
    ///     Revoke a specific session
    /// </summary>
    [Authorize]
    [HttpDelete("sessions/{tokenId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeSession(Guid tokenId)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized(new { message = "User not authenticated" });

        var success = await mediator.Send(new RevokeSessionCommand(CurrentUserId, tokenId));

        if (!success)
            return BadRequest(new { message = "Failed to revoke session" });

        return Ok(new { message = "Session revoked successfully" });
    }

    #endregion

    #region Private Helper Methods

    private void SetRefreshTokenCookie(string refreshToken, DateTime expirationDate)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevents JavaScript access
            Secure = true, // Only send over HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = expirationDate // Match refresh token expiration
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
    }

    #endregion
}