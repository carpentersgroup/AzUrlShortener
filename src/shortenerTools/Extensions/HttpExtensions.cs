using Shortener.Core.Redirect;

namespace ShortenerTools.Extensions
{
    public static class HttpExtensions
    {
        public static string GetHostFromRequest(this Microsoft.AspNetCore.Http.HttpRequest req)
        {
            // // Use custom header set in proxies or apim inbound policy to preserve the original host
            string? forwardedHost = req.GetFirstHeaderValue("X-Forwarded-Host");
            if (!string.IsNullOrWhiteSpace(forwardedHost))
            {
                return forwardedHost;
            }

            return req.Host.Value;
        }

        public static string GetHostFromRequest(this Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        {
            // Use custom header set in proxies.json or apim inbound policy to preserve the original host
            string? forwardedHost = req.GetFirstHeaderValue("X-Forwarded-Host");
            if (!string.IsNullOrWhiteSpace(forwardedHost))
            {
                return forwardedHost;
            }

            return req.Url.Host;
        }
    }
}
