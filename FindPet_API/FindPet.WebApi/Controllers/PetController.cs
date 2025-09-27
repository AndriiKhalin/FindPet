using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.CQRS.Queries.Pet;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.Domain.DTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PetController : ControllerBase
{
    private readonly IManageImage<Pet> _manageImage;
    private readonly IMediator _mediator;

    public PetController(IManageImage<Pet> manageImage, IMediator mediator)
    {
        _manageImage = manageImage;
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
    public async Task<IActionResult> UploadImage([FromForm] FileUploadDto file)
    {
        if (file == null || file.ImageFile.Length == 0)
            return BadRequest(new { error = "No file provided or file is empty" });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ImageFile.ContentType.ToLower()))
            return BadRequest(new { error = "Invalid file type. Only JPEG, PNG, and GIF are allowed." });

        var uniqueId = Guid.NewGuid();
        var filePath = await _manageImage.UploadPhotoAsync(file.ImageFile, uniqueId);

        return Ok(new { filePath, fileName = file.ImageFile.FileName });
    }
}