using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System.Text;
using FindPet.API;
using Microsoft.EntityFrameworkCore;
using FindPet.BusinessLogicLayer.Mappings;
using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Data.SeedData;
using FindPet.Infrastructure;
using FindPet.Infrastructure.Configurations.ServiceExtensions;

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCustomStaticFiles();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize data

await app.SeedAsync();
app.OpenLogFile();

app.Run();