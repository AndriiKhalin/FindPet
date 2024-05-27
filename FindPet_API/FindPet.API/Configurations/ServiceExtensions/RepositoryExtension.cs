using FindPet.Core.Repositories.EntityRepository;
using FindPet.Infrastructure.Interfaces.IEntityRepository;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class RepositoryExtension
{
    public static void ConfigureRepository(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}