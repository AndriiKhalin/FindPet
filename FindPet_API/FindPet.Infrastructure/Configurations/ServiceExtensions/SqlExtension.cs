using FindPet.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class SqlExtension
{
    public static void ConfigureMySqlContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["ConnectionStrings:AppDb"];
        services.AddDbContext<FindPetDbContext>(o => o.UseSqlServer(connectionString));
        //services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(connectionString));
    }
}