using Azure;
using FindPet.Domain.Entities;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FindPet.Domain.DTOs;
using FindPet.Domain.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FindPet.API.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class AccountController : ControllerBase
    //{
    //    private readonly UserManager<AuthUser> _userManager;
    //    private readonly RoleManager<IdentityRole> _roleManager;
    //    private readonly IConfiguration _configuration;


    //    public AccountController(UserManager<AuthUser> userManager,
    //        RoleManager<IdentityRole> roleManager,
    //        IConfiguration configuration
    //    )
    //    {
    //        _userManager = userManager;
    //        _roleManager = roleManager;
    //        _configuration = configuration;

    //    }

    //    [HttpPost]
    //    [Route("login")]
    //    public async Task<IActionResult> Login([FromBody] LoginDto login)
    //    {
    //        var user = await _userManager.FindByNameAsync(login.UserName);
    //        if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
    //        {
    //            var userRoles = await _userManager.GetRolesAsync(user);

    //            var authClaims = new List<Claim>
    //            {
    //                new Claim(ClaimTypes.Name, user.UserName),
    //                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //            };

    //            foreach (var userRole in userRoles)
    //            {
    //                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
    //            }

    //            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

    //            var token = new JwtSecurityToken(
    //                issuer: _configuration["JWT:ValidIssuer"],
    //                audience: _configuration["JWT:ValidAudience"],
    //                expires: DateTime.Now.AddHours(3),
    //                claims: authClaims,
    //                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
    //                );

    //            return Ok(new
    //            {
    //                token = new JwtSecurityTokenHandler().WriteToken(token),
    //                expiration = token.ValidTo
    //            });
    //        }
    //        return Unauthorized();
    //    }

    //    [HttpPost]
    //    [Route("register")]
    //    public async Task<IActionResult> Register(RegisterDto register)
    //    {
    //        var userExists = await _userManager.FindByNameAsync(register.UserName);
    //        if (userExists != null)
    //            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { IsSuccess = false, Message = "User already exists!" });

    //        var user = new AuthUser()
    //        {
    //            Email = register.Email,
    //            SecurityStamp = Guid.NewGuid().ToString(),
    //            UserName = register.UserName
    //        };
    //        var result = await _userManager.CreateAsync(user, register.Password);

    //        //if (!result.Succeeded)
    //        //    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { IsSuccess = false, Message = "User creation failed! Please check user details and try again." });
    //        if (!result.Succeeded)
    //        {
    //            return BadRequest(result.Errors);
    //        }

    //        return Ok(new AuthResponse
    //        {
    //            IsSuccess = true,
    //            Message = "Account Created Sucessfully!"
    //        });
    //    }

    //    [HttpPost]
    //    [Route("register-admin")]
    //    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto register)
    //    {
    //        var userExists = await _userManager.FindByNameAsync(register.UserName);
    //        if (userExists != null)
    //            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { IsSuccess = false, Message = "User already exists!" });

    //        AuthUser user = new AuthUser()
    //        {
    //            Email = register.Email,
    //            SecurityStamp = Guid.NewGuid().ToString(),
    //            UserName = register.UserName
    //        };
    //        var result = await _userManager.CreateAsync(user, register.Password);
    //        //if (!result.Succeeded)
    //        //    return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { IsSuccess = false, Message = "User creation failed! Please check user details and try again." });
    //        if (!result.Succeeded)
    //        {
    //            return BadRequest(result.Errors);
    //        }

    //        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
    //            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
    //        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
    //            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

    //        if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
    //        {
    //            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
    //        }

    //        return Ok(new AuthResponse { IsSuccess = true, Message = "User created successfully!" });
    //    }
    //}


    //////////////////////////////////////////////////////////////////////

    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AuthUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;


        public AccountController(UserManager<AuthUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;

        }

        // api/account/register

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AuthUser
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (registerDto.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }


            return Ok(new AuthResponse()
            {
                IsSuccess = true,
                Message = "Account Created Sucessfully!"
            });

        }

        //api/account/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null)
            {
                return Unauthorized(new AuthResponse()
                {
                    IsSuccess = false,
                    Message = "User not found with this email",
                });
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result)
            {
                return Unauthorized(new AuthResponse()
                {
                    IsSuccess = false,
                    Message = "Invalid Password."
                });
            }


            var token = GenerateToken(user);

            return Ok(new AuthResponse()
            {
                Token = token,
                IsSuccess = true,
                Message = "Login Success."
            });


        }


        private string GenerateToken(AuthUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII
            .GetBytes(_configuration.GetSection("JWT").GetSection("Secret").Value!);

            var roles = _userManager.GetRolesAsync(user).Result;

            List<Claim> claims =
            [
                new (JwtRegisteredClaimNames.Email,user.Email??""),
                new (JwtRegisteredClaimNames.Name,user.FullName??""),
                new (JwtRegisteredClaimNames.NameId,user.Id ??""),
                new (JwtRegisteredClaimNames.Aud,
                _configuration.GetSection("JWT").GetSection("ValidAudience").Value!),
                new (JwtRegisteredClaimNames.Iss,_configuration.GetSection("JWT").GetSection("ValidIssuer").Value!)
            ];


            foreach (var role in roles)

            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);


        }

        //api/account/detail
        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);


            if (user is null)
            {
                return NotFound(new AuthResponse()
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = [.. await _userManager.GetRolesAsync(user)],
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,

            });

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userManager.Users.Select(u => new UserDetailDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Roles = _userManager.GetRolesAsync(u).Result.ToArray()
            }).ToListAsync();

            return Ok(users);
        }


    }
}

