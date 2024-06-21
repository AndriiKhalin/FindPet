using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Infrastructure.Interfaces.IEntityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly IMapper _mapper;

        public PetController(IPetService petService, IMapper mapper)
        {
            _petService = petService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PetDto>))]
        public async Task<IActionResult> GetPets()
        {
            var pets = _mapper.Map<IEnumerable<PetDto>>(_petService.GetPets());

            return Ok(pets);
        }

        [HttpGet("{petId}")]
        [ProducesResponseType(200, Type = typeof(PetDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPet(Guid petId)
        {
            var pet = _mapper.Map<PetDto>(await _petService.GetPetAsync(petId));

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
        public async Task<IActionResult> CreatePet([FromQuery] Guid userId, [FromForm] PetForCreateDto petCreate)
        {

            var petMap = await _petService.CreatePetAsync(userId, petCreate);

            var createdPet = _mapper.Map<PetDto>(petMap);

            return CreatedAtAction(nameof(GetPet), new { petId = createdPet.Id }, createdPet);
        }


        [HttpPut("{petId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePet(Guid petId, [FromForm] PetForUpdateDto petUpdate)
        {

            await _petService.UpdatePetAsync(petId, petUpdate);

            return NoContent();
        }

        [HttpDelete("{petId}")]
        public async Task<IActionResult> DeletePet(Guid petId)
        {

            await _petService.DeletePetAsync(petId);

            return NoContent();

        }
    }
}
