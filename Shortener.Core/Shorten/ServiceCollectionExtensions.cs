using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shortener.Core.Configuration;
using Shortener.Core.Shorten.Algorithms;
using Shortener.Core.Shorten.Algorithms.CodeProviders;

namespace Shortener.Core.Shorten
{
    public static class ServiceCollectionExtensions
    {
        public static void AddUrlGeneration(this IServiceCollection services)
        {
            services.AddSingleton<IUrlGenerator, UrlGenerator>();
            services.AddSingleton(s =>
            {
                IOptions<UrlShortenerConfiguration> options = s.GetRequiredService<IOptions<UrlShortenerConfiguration>>();
                return AlphabetFactory.GetAlphabetProvider(options);
            });
            services.AddSingleton<IHash, HashGenerator>();
            services.AddSingleton<IEncode, Encoder>();
        }
    }
}