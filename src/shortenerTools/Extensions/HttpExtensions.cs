namespace ShortenerTools.Extensions
{
    public static class HttpExtensions
    {
        public static string GetHostFromRequest(this Microsoft.AspNetCore.Http.HttpRequest req)
        {
            return req.Host.Value;
        }

        public static string GetHostFromRequest(this Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        {
            return req.Url.Host;
        }
    }
}
