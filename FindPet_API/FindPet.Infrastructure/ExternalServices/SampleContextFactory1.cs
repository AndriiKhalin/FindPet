//using FindPet.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;

//namespace FindPet.Infrastructure.ExternalServices;

//public class SampleContextFactory1 : IDesignTimeDbContextFactory<AuthDbContext>
//{
//    public AuthDbContext CreateDbContext(string[] args)
//    {
//        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();



//        ConfigurationBuilder builder = new ConfigurationBuilder();
//        builder.SetBasePath(Directory.GetCurrentDirectory());
//        builder.AddJsonFile("appsettings.json");
//        IConfigurationRoot config = builder.Build();

//        // получаем строку подключения из файла appsettings.json
//        var connectionString = config.GetConnectionString("AppDb");
//        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("FindPet.API"));
//        return new AuthDbContext(optionsBuilder.Options);
//    }
//}