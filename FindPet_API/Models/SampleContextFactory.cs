using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Models;

public class SampleContextFactory : IDesignTimeDbContextFactory<FindPetDbContext>
{
    public FindPetDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FindPetDbContext>();



        ConfigurationBuilder builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json");
        IConfigurationRoot config = builder.Build();

        // получаем строку подключения из файла appsettings.json
        var connectionString = config.GetConnectionString("AppDb");
        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("API"));
        return new FindPetDbContext(optionsBuilder.Options);
    }
}