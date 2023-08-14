using System.Net;

namespace Shortener.Core.Redirect
{
    public interface IUrlRedirectService
    {
        Task<Result<string, RedirectStatus>> ProcessAsync(IPAddress? ipAddress, string host, string shortUrl);
    }
}
