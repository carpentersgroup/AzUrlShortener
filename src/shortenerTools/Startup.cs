using Azure.Data.Tables;
using Fizzibly.Auth;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shortener.AzureServices;
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
            //TODO: Fix this cold start 14second business!!!
            //TODO: Lets try the tree shaking thing
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

            services.AddSingleton<TableServiceClient>(s =>
            {
                TableServiceClient serviceClient = new TableServiceClient(configuration.GetSection("UlsDataStorage").Value);
                return serviceClient;
            });

            services.AddSingleton<IStorageTableHelper, StorageTableHelper>();
            services.AddSingleton<IMigrationTableHelper, MigrationTableHelper>();

            services.AddUrlGeneration();
        }
    }
}