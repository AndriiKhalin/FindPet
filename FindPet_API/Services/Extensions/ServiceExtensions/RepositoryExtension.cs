using Interfaces.IEntityRepository;

using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.EntityRepository;

namespace Services.Extensions.ServiceExtensions;

public static class RepositoryExtension
{
    public static void ConfigureRepository(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}