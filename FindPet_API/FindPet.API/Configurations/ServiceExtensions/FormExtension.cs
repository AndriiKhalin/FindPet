using Microsoft.AspNetCore.Http.Features;

namespace FindPet.API.Configurations.ServiceExtensions;

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