using AutoMapper;
using Interfaces.IEntityService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.OwnerDTO;
using Models.Entities;
using Services.Service.EntityService;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnerService _ownerService;
        private readonly IMapper _mapper;

        public OwnerController(IOwnerService ownerService, IMapper mapper)
        {
            _ownerService = ownerService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<OwnerDto>))]
        public async Task<IActionResult> GetOwners()
        {
            var owners = _mapper.Map<IEnumerable<OwnerDto>>(await _ownerService.GetOwnersAsync());

            return Ok(owners);
        }

        [HttpGet("{ownerId}")]
        [ProducesResponseType(200, Type = typeof(OwnerDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetOwner(Guid ownerId)
        {
            var owner = _mapper.Map<OwnerDto>(await _ownerService.GetOwnerAsync(ownerId));

            return Ok(owner);
        }

        //[HttpGet("{OwnerId}/orders")]
        //[ProducesResponseType(200, Type = typeof(IEnumerable<OrderDto>))]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> GetOrdersByOwner(Guid OwnerId)
        //{
        //    var ordersByOwner = _mapper.Map<IEnumerable<OrderDto>>(await _OwnerService.GetOrdersByOwner(OwnerId));
        //    return Ok(ordersByOwner);
        //}

        //[HttpGet("categories/{OwnerId}")]
        //[ProducesResponseType(200, Type = typeof(OwnerCategoryDto))]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> GetCategoryByOwner(Guid OwnerId)
        //{

        //    var categoryByOwner = _mapper.Map<OwnerCategoryDto>(await _OwnerService.GetCategoryByOwner(OwnerId));

        //    return Ok(categoryByOwner);
        //}

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(OwnerDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOwner([FromForm] OwnerForCreateDto ownerCreate)
        {

            var ownerMap = await _ownerService.CreateOwnerAsync(ownerCreate);

            var createdOwner = _mapper.Map<OwnerDto>(ownerMap);

            return CreatedAtAction(nameof(GetOwner), new { ownerId = createdOwner.Id }, createdOwner);
        }


        [HttpPut("{ownerId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOwner(Guid ownerId, [FromForm] OwnerForUpdateDto ownerUpdate)
        {

            await _ownerService.UpdateOwnerAsync(ownerId, ownerUpdate);

            return NoContent();
        }

        [HttpDelete("{ownerId}")]
        public async Task<IActionResult> DeleteOwner(Guid ownerId)
        {

            await _ownerService.DeleteOwnerAsync(ownerId);

            return NoContent();

        }
    }
}
