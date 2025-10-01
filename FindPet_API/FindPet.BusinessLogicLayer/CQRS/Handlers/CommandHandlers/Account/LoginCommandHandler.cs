using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        UserManager<AuthUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.LoginDto.Email);

        if (user is null)
            throw new UnauthorizedException("User not found with this email");

        var result = await _userManager.CheckPasswordAsync(user, request.LoginDto.Password);

        if (!result)
            throw new UnauthorizedException("Invalid password");

        var token = await GenerateTokenAsync(user);

        return new AuthResponse
        {
            Token = token,
            IsSuccess = true,
            Message = "Login success"
        };
    }

    private async Task<string> GenerateTokenAsync(AuthUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JWT:Secret").Value!);
        var roles = await _userManager.GetRolesAsync(user);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Name, user.Name ?? ""),
            new(JwtRegisteredClaimNames.NameId, user.Id ?? ""),
            new(JwtRegisteredClaimNames.Aud, _configuration.GetSection("JWT:ValidAudience").Value!),
            new(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWT:ValidIssuer").Value!)
        ];

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

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
}