using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FindPet.Infrastructure.Configurations.AuthExtensions;

public static class JwtAuthenticationExtension
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JWT");

        services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.RequireHttpsMetadata = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = jwtSection["ValidAudience"],
                    ValidIssuer = jwtSection["ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Secret"]!))
                };
            });
    }
}