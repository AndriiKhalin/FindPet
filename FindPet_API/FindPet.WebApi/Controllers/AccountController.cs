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

    // api/account/register

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

    //api/account/login

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
    ///     Refresh access token using refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        if (string.IsNullOrEmpty(RefreshTokenFromCookie))
            return Unauthorized("Refresh token is required");

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
}