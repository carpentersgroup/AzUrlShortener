using Fizzibly.Auth;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shortener.Azure;
using Shortener.Core.Configuration;
using Shortener.Core.Redirect;
using Shortener.Core.Shorten;
using ShortenerTools;
using System;
using System.Diagnostics.CodeAnalysis;


[assembly: FunctionsStartup(typeof(Startup))]
namespace ShortenerTools
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services, builder.GetContext().Configuration);
        }

        internal static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging();

            services.Configure<UrlShortenerConfiguration>(configuration.GetSection(UrlShortenerConfiguration.KEY));

            services.AddSingleton<IUrlRedirectService, UrlRedirectService>();
            services.AddSingleton<IUrlShortenerService, UrlShortenerService>();

            services.Configure<Fizzibly.Auth.JwtSettings>(configuration.GetSection(Fizzibly.Auth.JwtSettings.KEY));
            services.AddAuthHandlers();
            services.RegisterAuthHandlers();

            services.AddHttpClient<IUserIpLocationService, UserIpLocationService>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("IpLocationService:Url").Value);
            });

            services.AddSingleton<IStorageTableHelper, StorageTableHelper>(provider =>
            {
                var storageAccount = CloudStorageAccount.Parse(configuration.GetSection("UlsDataStorage").Value);
                var tableClient = storageAccount.CreateCloudTableClient();
                var logger = provider.GetRequiredService<ILogger<StorageTableHelper>>();

                var storageHelper = new StorageTableHelper(tableClient, logger);
                return storageHelper;
            });

            services.AddUrlGeneration();
        }
    }
}