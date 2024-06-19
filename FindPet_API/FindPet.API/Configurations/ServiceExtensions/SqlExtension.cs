using FindPet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class SqlExtension
{
    public static void ConfigureMySqlContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["ConnectionStrings:AppDb"];
        services.AddDbContext<FindPetDbContext>(o => o.UseSqlServer(connectionString));
        services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(connectionString));

    }
}