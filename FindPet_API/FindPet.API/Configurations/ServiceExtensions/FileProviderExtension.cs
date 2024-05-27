using Microsoft.Extensions.FileProviders;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class FileProviderExtension
{
    public static void Configure_FileProvider(this IServiceCollection services)
    {
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(GetPath()));
    }

    public static void UseCustomStaticFiles(this IApplicationBuilder app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(GetPath()),
            RequestPath = @"/Images"
        });
    }
    private static string GetPath()
    {
        //return @"D:\IT\My_Projects\RentShop\RentShop_UI\Stuff\Images";

        var currentDirectory = Directory.GetCurrentDirectory();
        var projectRoot = Path.GetFullPath(Path.Combine(currentDirectory, "..", ".."));
        var pathToImages = Path.Combine(projectRoot, @"FindPet_UI\src\assets\");

        return pathToImages;
    }
}