using FindPet.Email.Configuration;
using FindPet.Email.Interfaces;
using FindPet.Email.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FindPet.Email;

public static class EmailLayerDI
{
    public static void AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure EmailSettings from appsettings.json
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<EmailSettings>>().Value);

        services.AddScoped<IEmailService, GmailEmailService>();


        // Register Brevo API
        //services.AddScoped<TransactionalEmailsApi>(sp =>
        //{
        //    var settings = sp.GetRequiredService<EmailSettings>();

        //    // Use indexer to set (replaces if exists) instead of Add
        //    BrevoConfig.Default.ApiKey["api-key"] = settings.ApiKey;

        //    return new TransactionalEmailsApi();
        //});

        //// Register Email Service
        //services.AddScoped<IEmailService, BrevoEmailService>();
    }
}