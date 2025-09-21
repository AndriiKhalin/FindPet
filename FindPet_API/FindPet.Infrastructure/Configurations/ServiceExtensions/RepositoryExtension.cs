using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.DataAccessLayer.Repositories.EntityRepository;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class RepositoryExtension
{
    public static void ConfigureRepository(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}