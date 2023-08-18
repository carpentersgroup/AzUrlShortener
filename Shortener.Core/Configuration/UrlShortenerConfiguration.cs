using Shortener.Core.Shorten.Algorithms;
using Shortener.Core.Shorten.Algorithms.CodeProviders;
using System.Diagnostics.CodeAnalysis;

namespace Shortener.Core.Configuration
{
    public class UrlShortenerConfiguration
    {
        public const string KEY = "UrlShortener";

        public bool EnableApiAccess { get; set; }
        public string? UrlShortenApiRoleName { get; set; }
        public Uri? CustomDomain { get; set; }
        [MemberNotNullWhen(returnValue: true, nameof(CustomDomain))]
        public bool UseCustomDomain => CustomDomain is not null;
        public string? DefaultRedirectUrl { get; set; }
        public ShortenerAlgorithm DefaultAlgorithm { get; set; } = ShortenerAlgorithm.RandomExtendableLength;
        public Alphabet Alphabet { get; set; } = Alphabet.NoProfanity;
        public string? CustomAlphabet { get; set; }
        public bool RecordCountryStats { get; set; } = true;
    }
}
