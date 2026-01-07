using FindPet.Infrastructure;
using FindPet.Infrastructure.Configurations.ServiceExtensions;
using FindPet.WebApi;
using FindPet.WebApi.Hubs;
using FindPet.WebApi.Middlewares;
using NLog;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAPIServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions())
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"FindPet API {description.GroupName.ToUpperInvariant()}");
        options.DocExpansion(DocExpansion.None);
        options.EnableValidator();
    });
}

app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCustomStaticFiles();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

// Initialize app
app.OpenLogFile();

app.Run();