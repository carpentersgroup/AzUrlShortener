using Shortener.Core.Shorten.Algorithms;

namespace Shortener.Core.Shorten
{
    public interface IUrlGenerator
    {
        Task<(string Vanity, ShortenerAlgorithm Algorithm)> GenerateAsync(string? host);
    }
}