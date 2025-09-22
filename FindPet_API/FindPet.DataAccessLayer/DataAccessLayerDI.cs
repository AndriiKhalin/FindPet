using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.DataAccessLayer.Repositories.EntityRepository;
using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.DataAccessLayer;

public static class DataAccessLayerDI
{
    public static void AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<FindPetDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AppDb")));
        //services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(connectionString));

        // Repository Pattern
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IAdRepository, AdRepository>();
        services.AddScoped<IUserRepository<User>, UserRepository>();
        //services.AddScoped<IFinderRepository, FinderRepository>();
        //services.AddScoped<IOwnerRepository, OwnerRepository>();
    }
}