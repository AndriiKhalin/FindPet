using FindPet.BusinessLogicLayer.CQRS.Commands.Image;
using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.CQRS.Queries.User;
using FindPet.Domain.DTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.DTOs.FileDTOs;
using FindPet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserDto>))]
    public async Task<IActionResult> GetUsers()
    {
        var users = await mediator.Send(new GetAllUsersQuery());

        return Ok(users);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userId));

        return Ok(user);
    }

    //[HttpGet("{UserId}/orders")]
    //[ProducesResponseType(200, Type = typeof(IEnumerable<OrderDto>))]
    //[ProducesResponseType(400)]
    //public async Task<IActionResult> GetOrdersByUser(Guid UserId)
    //{
    //    var ordersByUser = _mapper.Map<IEnumerable<OrderDto>>(await _UserService.GetOrdersByUser(UserId));
    //    return Ok(ordersByUser);
    //}

    //[HttpGet("categories/{UserId}")]
    //[ProducesResponseType(200, Type = typeof(UserCategoryDto))]
    //[ProducesResponseType(400)]
    //public async Task<IActionResult> GetCategoryByUser(Guid UserId)
    //{
    //    var categoryByUser = _mapper.Map<UserCategoryDto>(await _UserService.GetCategoryByUser(UserId));

    //    return Ok(categoryByUser);
    //}

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUser(UserForCreateDto userCreate)
    {
        var createdUser = await mediator.Send(new CreateUserCommand(userCreate));

        return CreatedAtAction(nameof(GetUser), new { userId = createdUser.Id }, createdUser);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateUser(Guid userId, UserForUpdateDto userUpdate)
    {
        await mediator.Send(new UpdateUserCommand(userId, userUpdate));

        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await mediator.Send(new DeleteUserCommand(userId));

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("uploadImage")]
    [DisableRequestSizeLimit]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadDto file)
    {
        var response = await mediator.Send(new UploadImageCommand(file.File, EntityType.User));
        return Ok(response);
    }
}