using Microsoft.Extensions.Options;
using Shortener.Core.Configuration;

namespace Shortener.Core.Shorten.Algorithms.CodeProviders
{
    public static class AlphabetFactory
    {
        public static AlphabetProviderBase GetAlphabetProvider(IOptions<UrlShortenerConfiguration> options)
        {
            if(options.Value.Alphabet == Alphabet.Custom && string.IsNullOrEmpty(options.Value.CustomAlphabet))
                throw new ArgumentException("CustomAlphabet is not defined in settings. Please define it (e.g. \"UrlShortener:CustomAlphabet\": \"-023456789ABCDEFGHJKLMNOPQRSTVWXYZ_abcdefghjklmnopqrstvwxyz\"");

            return options.Value.Alphabet switch
            {
                Alphabet.NoProfanity => new NoProfanityAlphabetProvider(),
                Alphabet.Base64 => new Base64AlphabetProvider(),
                Alphabet.Custom => new CustomAlphabetProvider(options.Value.CustomAlphabet!),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}