using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Data.SeedData;

public static class TasksInitializer
{
    public static async Task<WebApplication> SeedAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {

            using var context = scope.ServiceProvider.GetRequiredService<FindPetDbContext>();
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedData.SeedDatesAsync(context, roleManager);

        }
        return app;
    }
}