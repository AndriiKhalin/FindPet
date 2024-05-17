using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;

namespace Services.Extensions.ServiceExtensions;

public static class SqlExtension
{
    public static void ConfigureMySqlContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["ConnectionStrings:AppDb"];
        services.AddDbContext<FindPetDbContext>(o => o.UseSqlServer(connectionString));
    }
}