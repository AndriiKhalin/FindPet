using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class FormExtension
{
    public static void ConfigureForm(this IServiceCollection services)
    {
        //services.AddCors(options =>
        //{
        //    options.AddPolicy("CorsPolicy",
        //        builder => builder.AllowAnyOrigin()
        //            .AllowAnyMethod()
        //            .AllowAnyHeader());
        //});
        services.Configure<FormOptions>(o =>
        {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = int.MaxValue;
            o.MemoryBufferThreshold = int.MaxValue;
        });
    }
}