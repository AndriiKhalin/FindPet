using Models.SeedData;
using NLog;
using Services.Extensions.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));


// Add services to the container.
builder.Services.ConfigureMySqlContext(builder.Configuration);



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
//app.UseCustomStaticFiles();
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();
app.Seed();
app.Run();
