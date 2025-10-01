using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FindPet.BusinessLogicLayer.CQRS.Queries.Ad;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers;

//[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AdController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<AdDto>))]
    public async Task<IActionResult> GetAds()
    {
        var ads = await _mediator.Send(new GetAllAdsQuery());

        return Ok(ads);
    }

    [HttpGet("{adId}")]
    [ProducesResponseType(200, Type = typeof(AdDto))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAd(Guid adId)
    {
        var ad = await _mediator.Send(new GetAdByIdQuery(adId));

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
    public async Task<IActionResult> CreateAd([FromQuery] Guid petId, [FromQuery] Guid userId,
        [FromForm] AdForCreateDto adCreate)
    {
        var createdAd = await _mediator.Send(new CreateAdCommand(petId, userId, adCreate));

        return CreatedAtAction(nameof(GetAd), new { adId = createdAd.Id }, createdAd);
    }

    [HttpPut("{adId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAd(Guid adId, [FromForm] AdForUpdateDto adUpdate)
    {
        await _mediator.Send(new UpdateAdCommand(adId, adUpdate));

        return NoContent();
    }

    [HttpDelete("{adId}")]
    public async Task<IActionResult> DeleteAd(Guid adId)
    {
        await _mediator.Send(new DeleteAdCommand(adId));

        return NoContent();
    }
}