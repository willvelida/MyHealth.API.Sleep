using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyHealth.API.Sleep;
using MyHealth.API.Sleep.Services;
using MyHealth.API.Sleep.Validators;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyHealth.API.Sleep
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddLogging();

            builder.Services.AddSingleton(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                return new CosmosClient(configuration["CosmosDBConnectionString"]);
            });

            builder.Services.AddScoped<ISleepDbService, SleepDbService>();
            builder.Services.AddScoped<IDateValidator, DateValidator>();
        }
    }
}
