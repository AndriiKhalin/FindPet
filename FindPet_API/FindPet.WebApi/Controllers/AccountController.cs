using System.Security.Claims;
using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator) : ControllerBase
{
    // api/account/register

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await mediator.Send(new RegisterCommand(registerDto));
        return Ok(response);
    }

    //api/account/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await mediator.Send(new LoginCommand(loginDto));
        return Ok(response);
    }

    //api/account/detail
    [HttpGet("detail")]
    public async Task<ActionResult<UserDetailDto>> GetUserDetail()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var userDetail = await mediator.Send(new GetUserDetailQuery(currentUserId));
        return Ok(userDetail);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
    {
        var users = await mediator.Send(new GetAllAuthUsersQuery());
        return Ok(users);
    }
}