using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AuthUser> _userManager;

    public LoginCommandHandler(
        UserManager<AuthUser> userManager,
        IConfiguration configuration,
        IAuthService authService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.LoginDto, cancellationToken);
        //var user = await _userManager.FindByEmailAsync(request.LoginDto.Email);

        //if (user is null)
        //    throw new UnauthorizedException("User not found with this email");

        //var result = await _userManager.CheckPasswordAsync(user, request.LoginDto.Password);

        //if (!result)
        //    throw new UnauthorizedException("Invalid password");

        //var token = await GenerateTokenAsync(user);

        //return new AuthResponse
        //{
        //    AccessToken = token,
        //    IsSuccess = true,
        //    Message = "Login success"
        //};
    }

    //private async Task<string> GenerateTokenAsync(AuthUser user)
    //{
    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JWT:Secret").Value!);
    //    var roles = await _userManager.GetRolesAsync(user);

    //    List<Claim> claims =
    //    [
    //        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
    //        new(JwtRegisteredClaimNames.Name, user.Name ?? ""),
    //        new(JwtRegisteredClaimNames.NameId, user.Id ?? ""),
    //        new(JwtRegisteredClaimNames.Aud, _configuration.GetSection("JWT:ValidAudience").Value!),
    //        new(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWT:ValidIssuer").Value!)
    //    ];

    //    foreach (var role in roles)
    //        claims.Add(new Claim(ClaimTypes.Role, role));

    //    var tokenDescriptor = new SecurityTokenDescriptor
    //    {
    //        Subject = new ClaimsIdentity(claims),
    //        Expires = DateTime.UtcNow.AddDays(1),
    //        SigningCredentials = new SigningCredentials(
    //            new SymmetricSecurityKey(key),
    //            SecurityAlgorithms.HmacSha256
    //        )
    //    };

    //    var token = tokenHandler.CreateToken(tokenDescriptor);
    //    return tokenHandler.WriteToken(token);
    //}
}