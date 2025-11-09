using System.Security.Claims;
using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator, ITokenService tokenService) : ControllerBase
{
    private string GetRefreshTokenFromCookie => Request.Cookies["refreshToken"] ?? string.Empty;
    private string GetClientIpAddress => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    // api/account/register

    /// <summary>
    /// Register a new user
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
    /// Login with email and password
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
        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        var refreshToken = GetRefreshTokenFromCookie;
        var ipAddress = GetClientIpAddress;

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Refresh token is required");

        var response = await mediator.Send(new RefreshTokenCommand(refreshToken, ipAddress));

        //SetRefreshTokenCookie(response.RefreshToken!);

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
    /// Logout (revoke refresh token)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = GetRefreshTokenFromCookie;
        var ipAddress = GetClientIpAddress;

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        }

        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user details
    /// </summary>
    [Authorize]
    [HttpGet("detail")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDetailDto>> GetUserDetail()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var userDetail = await mediator.Send(new GetUserDetailQuery(currentUserId));
        return Ok(userDetail);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
    {
        var users = await mediator.Send(new GetAllAuthUsersQuery());
        return Ok(users);
    }
}