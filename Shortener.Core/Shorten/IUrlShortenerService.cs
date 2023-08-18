namespace Shortener.Core.Shorten
{
    public interface IUrlShortenerService
    {
        Task<Result<ShortResponse, ShortnerStatus>> ShortenUrl(ShortRequest shortRequest);
    }
}