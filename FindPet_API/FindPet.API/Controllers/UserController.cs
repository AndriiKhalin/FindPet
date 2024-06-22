using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Infrastructure.Interfaces.IEntityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UserDto>))]
        public async Task<IActionResult> GetUsers()
        {
            var users = _mapper.Map<IEnumerable<UserDto>>(_userService.GetUsers());

            return Ok(users);
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(200, Type = typeof(UserDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var user = _mapper.Map<UserDto>(await _userService.GetUserAsync(userId));

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
        public async Task<IActionResult> CreateUser([FromForm] UserForCreateDto userCreate)
        {

            var userMap = await _userService.CreateUserAsync(userCreate);

            var createdUser = _mapper.Map<UserDto>(userMap);

            return CreatedAtAction(nameof(GetUser), new { userId = createdUser.Id }, createdUser);
        }


        [HttpPut("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromForm] UserForUpdateDto userUpdate)
        {

            await _userService.UpdateUserAsync(userId, userUpdate);

            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {

            await _userService.DeleteUserAsync(userId);

            return NoContent();

        }
    }
}
