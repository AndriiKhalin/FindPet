using FindPet.BusinessLogicLayer.CQRS.Commands.Role;
using FindPet.BusinessLogicLayer.CQRS.Queries.Role;
using FindPet.Domain.DTOs.AuthDTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        var result = await mediator.Send(new CreateRoleCommand(createRoleDto));

        if (result.Succeeded) return Ok(new { message = "Role Created successfully" });

        return BadRequest("Role creation failed.");
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RoleResponseDto>))]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await mediator.Send(new GetAllRolesQuery());

        return Ok(roles);
    }

    //[Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(200, Type = typeof(object))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id));

        if (result.Succeeded) return Ok(new { message = "Role deleted successfully." });

        return BadRequest("Role deletion failed.");
    }

    [HttpPost("assign")]
    [ProducesResponseType(200, Type = typeof(object))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto roleAssignDto)
    {
        var result = await mediator.Send(new AssignRoleCommand(roleAssignDto));

        if (result.Succeeded) return Ok(new { message = "Role assigned successfully" });

        var error = result.Errors.FirstOrDefault();

        return BadRequest(error!.Description);
    }
}