using AutoMapper;
using FindPet.Domain.DTOs.AdDTO;
using FindPet.Infrastructure.Interfaces.IEntityService;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdController : ControllerBase
    {
        private readonly IAdService _adService;
        private readonly IMapper _mapper;

        public AdController(IAdService adService, IMapper mapper)
        {
            _adService = adService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AdDto>))]
        public async Task<IActionResult> GetAds()
        {
            var ads = _mapper.Map<IEnumerable<AdDto>>(_adService.GetAds());

            return Ok(ads);
        }

        [HttpGet("{adId}")]
        [ProducesResponseType(200, Type = typeof(AdDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAd(Guid adId)
        {
            var ad = _mapper.Map<AdDto>(await _adService.GetAdAsync(adId));

            return Ok(ad);
        }

        //[HttpGet("{AdId}/orders")]
        //[ProducesResponseType(200, Type = typeof(IEnumerable<OrderDto>))]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> GetOrdersByAd(Guid AdId)
        //{
        //    var ordersByAd = _mapper.Map<IEnumerable<OrderDto>>(await _AdService.GetOrdersByAd(AdId));
        //    return Ok(ordersByAd);
        //}

        //[HttpGet("categories/{AdId}")]
        //[ProducesResponseType(200, Type = typeof(AdCategoryDto))]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> GetCategoryByAd(Guid AdId)
        //{

        //    var categoryByAd = _mapper.Map<AdCategoryDto>(await _AdService.GetCategoryByAd(AdId));

        //    return Ok(categoryByAd);
        //}

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(AdDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateAd([FromQuery] Guid petId, [FromQuery] Guid userId, [FromForm] AdForCreateDto adCreate)
        {

            var adMap = await _adService.CreateAdAsync(petId, userId, adCreate);

            var createdAd = _mapper.Map<AdDto>(adMap);

            return CreatedAtAction(nameof(GetAd), new { adId = createdAd.Id }, createdAd);
        }


        [HttpPut("{adId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAd(Guid adId, [FromForm] AdForUpdateDto adUpdate)
        {

            await _adService.UpdateAdAsync(adId, adUpdate);

            return NoContent();
        }

        [HttpDelete("{adId}")]
        public async Task<IActionResult> DeleteAd(Guid adId)
        {

            await _adService.DeleteAdAsync(adId);

            return NoContent();

        }
    }
}
