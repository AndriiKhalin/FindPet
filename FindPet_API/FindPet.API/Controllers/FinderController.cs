using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.FinderDTO;
using FindPet.Infrastructure.Interfaces.IEntityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FinderController : ControllerBase
    {
        private readonly IFinderService _finderService;
        private readonly IMapper _mapper;

        public FinderController(IFinderService finderService, IMapper mapper)
        {
            _finderService = finderService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<FinderDto>))]
        public async Task<IActionResult> GetFinders()
        {
            var finders = _mapper.Map<IEnumerable<FinderDto>>(_finderService.GetFinders());

            return Ok(finders);
        }

        [HttpGet("{finderId}")]
        [ProducesResponseType(200, Type = typeof(FinderDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFinder(Guid finderId)
        {
            var finder = _mapper.Map<FinderDto>(await _finderService.GetFinderAsync(finderId));

            return Ok(finder);
        }

        //[HttpGet("{FinderId}/orders")]
        //[ProducesResponseType(200, Type = typeof(IEnumerable<OrderDto>))]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> GetOrdersByFinder(Guid FinderId)
        //{
        //    var ordersByFinder = _mapper.Map<IEnumerable<OrderDto>>(await _FinderService.GetOrdersByFinder(FinderId));
        //    return Ok(ordersByFinder);
        //}

        //[HttpGet("categories/{FinderId}")]
        //[ProducesResponseType(200, Type = typeof(FinderCategoryDto))]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> GetCategoryByFinder(Guid FinderId)
        //{

        //    var categoryByFinder = _mapper.Map<FinderCategoryDto>(await _FinderService.GetCategoryByFinder(FinderId));

        //    return Ok(categoryByFinder);
        //}

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(FinderDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateFinder([FromForm] FinderForCreateDto finderCreate)
        {

            var finderMap = await _finderService.CreateFinderAsync(finderCreate);

            var createdFinder = _mapper.Map<FinderDto>(finderMap);

            return CreatedAtAction(nameof(GetFinder), new { finderId = createdFinder.Id }, createdFinder);
        }


        [HttpPut("{finderId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateFinder(Guid finderId, [FromForm] FinderForUpdateDto finderUpdate)
        {

            await _finderService.UpdateFinderAsync(finderId, finderUpdate);

            return NoContent();
        }

        [HttpDelete("{finderId}")]
        public async Task<IActionResult> DeleteFinder(Guid finderId)
        {

            await _finderService.DeleteFinderAsync(finderId);

            return NoContent();

        }
    }
}
