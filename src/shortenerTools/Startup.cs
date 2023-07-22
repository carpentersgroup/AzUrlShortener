using Cloud5mins.domain;
using Cloud5mins.Function;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using shortenerTools;
using shortenerTools.Abstractions;
using shortenerTools.Implementations;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(Startup))]
namespace shortenerTools
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.Configure<UrlShortenerConfiguration>(config =>
            {
                config.CustomDomain = configuration.GetValue<string>("customDomain");
                config.UrlShortenApiRoleName = configuration.GetValue<string>("urlShortenApiRoleName");
                config.EnableApiAccess = configuration.GetValue<bool>("enableApiAccess");
                config.Code = configuration.GetValue<string>("code");
            });

            builder.Services.AddHttpClient<IUserIpLocationService, UserIpLocationService>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("IpLocationService:Url").Value);
            });

            builder.Services.AddSingleton<IStorageTableHelper, StorageTableHelper>(provider =>
            {
                var storageAccount = CloudStorageAccount.Parse(configuration.GetSection("UlsDataStorage").Value);
                var tableClient = storageAccount.CreateCloudTableClient();
                
                var storageHelper = new StorageTableHelper(tableClient);
                return storageHelper;
            });
        }
    }
}