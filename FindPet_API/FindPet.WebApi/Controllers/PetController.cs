using FindPet.BusinessLogicLayer.CQRS.Commands.Image;
using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.CQRS.Queries.Pet;
using FindPet.Domain.DTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PetController : ControllerBase
{
    private readonly IMediator _mediator;

    public PetController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<PetDto>))]
    public async Task<IActionResult> GetPets()
    {
        var pets = await _mediator.Send(new GetAllPetsQuery());
        return Ok(pets);
    }

    [AllowAnonymous]
    [HttpGet("{petId}")]
    [ProducesResponseType(200, Type = typeof(PetDto))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetPet(Guid petId)
    {
        var pet = await _mediator.Send(new GetPetByIdQuery(petId));
        return Ok(pet);
    }

    //[HttpGet("{PetId}/orders")]
    //[ProducesResponseType(200, Type = typeof(IEnumerable<OrderDto>))]
    //[ProducesResponseType(400)]
    //public async Task<IActionResult> GetOrdersByPet(Guid PetId)
    //{
    //    var ordersByPet = _mapper.Map<IEnumerable<OrderDto>>(await _petService.GetOrdersByPet(PetId));
    //    return Ok(ordersByPet);
    //}

    //[HttpGet("categories/{PetId}")]
    //[ProducesResponseType(200, Type = typeof(PetCategoryDto))]
    //[ProducesResponseType(400)]
    //public async Task<IActionResult> GetCategoryByPet(Guid PetId)
    //{
    //    var categoryByPet = _mapper.Map<PetCategoryDto>(await _petService.GetCategoryByPet(PetId));

    //    return Ok(categoryByPet);
    //}

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(PetDto))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreatePet([FromQuery] Guid userId, PetForCreateDto petCreate)
    {
        var createdPet = await _mediator.Send(new CreatePetCommand(userId, petCreate));
        return CreatedAtAction(nameof(GetPet), new { petId = createdPet.Id }, createdPet);
    }

    [HttpPut("{petId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePet(Guid petId, PetForUpdateDto petUpdate)
    {
        await _mediator.Send(new UpdatePetCommand(petId, petUpdate));
        return NoContent();
    }

    [HttpDelete("{petId}")]
    public async Task<IActionResult> DeletePet(Guid petId)
    {
        await _mediator.Send(new DeletePetCommand(petId));
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("uploadImage")]
    [DisableRequestSizeLimit]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(200, Type = typeof(UploadImageResponse))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadDto file)
    {
        var response = await _mediator.Send(new UploadImageCommand(file.ImageFile, EntityType.Pet));
        return Ok(response);
    }
}