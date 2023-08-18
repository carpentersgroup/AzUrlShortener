namespace Shortener.Core.Redirect
{
    public static class HttpHeaderExtensions
    {

        public static string? GetFirstHeaderValue(this Microsoft.Azure.Functions.Worker.Http.HttpRequestData req, string headerName)
        {
            return req.Headers.FirstOrDefault(x => x.Key == headerName).Value.FirstOrDefault();
        }

        public static string? GetFirstHeaderValue(this Microsoft.AspNetCore.Http.HttpRequest req, string headerName)
        {
            if (!req.Headers.TryGetValue(headerName, out var header))
            {
                return null;
            }

            var data = header[0];

            return data;
        }
    }
}