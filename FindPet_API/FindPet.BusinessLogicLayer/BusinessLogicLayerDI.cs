using FindPet.BusinessLogicLayer.CQRS.Behaviors;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.BusinessLogicLayer.Services.ImageService;
using FindPet.BusinessLogicLayer.Services.MLService;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FindPet.BusinessLogicLayer;

public static class BusinessLogicLayerDI
{
    public static void AddBusinessLogicServices(this IServiceCollection services)
    {
        // Entity Services
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IAdService, AdService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped(typeof(IManageImage<>), typeof(ManageImage<>));
        //services.AddScoped<IFinderService, FinderService>();
        //services.AddScoped<IOwnerService, OwnerService>();

        // ML Services
        services.AddScoped<IMLService, MLService>();

        // AutoMapper
        //services.AddAutoMapper(typeof(Mapping));

        services.AddCQRS();

        //services.AddScoped<RoleManager<IdentityRole>>();
    }

    private static void AddCQRS(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(assembly); });

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    }
}