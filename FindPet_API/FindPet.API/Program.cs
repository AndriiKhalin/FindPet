using FindPet.API.Configurations;
using FindPet.API.Configurations.ServiceExtensions;
using FindPet.Core.Mappings;
using FindPet.Domain.ValueObjects;
using FindPet.Infrastructure.Data;
using FindPet.Infrastructure.Data.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

var JWT = builder.Configuration.GetSection("JWT");


//builder.Services.AddIdentity<AuthUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>()
//    .AddDefaultTokenProviders();

//builder.Services.AddAuthentication(opt =>
//{
//    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(opt =>
//{
//    opt.SaveToken = true;
//    opt.RequireHttpsMetadata = false;
//    opt.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidAudience = JWT["ValidAudience"],
//        ValidIssuer = JWT["ValidIssuer"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT.GetSection("Secret").Value!))
//    };
//});



// Add services to the container.
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureMySqlContext(builder.Configuration);
builder.Services.ConfigureRepository();
builder.Services.ConfigureServices();
builder.Services.ConfigureManageImage();
builder.Services.AddAutoMapper(typeof(Mapping));
builder.Services.Configure_FileProvider();
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});


builder.Services.AddIdentity<AuthUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = JWT["ValidAudience"],
        ValidIssuer = JWT["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT.GetSection("Secret").Value!))
    };
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();




builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization Example : 'Bearer eyeleieieekeieieie",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement(){
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "outh2",
                Name="Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseStaticFiles();
app.UseCustomStaticFiles();
app.UseCors("CorsPolicy");


app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Seed();
app.OpenLogFile();

app.Run();
