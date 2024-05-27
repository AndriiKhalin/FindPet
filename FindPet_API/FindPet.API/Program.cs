using FindPet.API.Configurations.ServiceExtensions;
using FindPet.Core.Mappings;
using FindPet.Infrastructure.Data.SeedData;
using NLog;

var builder = WebApplication.CreateBuilder(args);
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));


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



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Seed();
app.OpenLogFile();

app.Run();
