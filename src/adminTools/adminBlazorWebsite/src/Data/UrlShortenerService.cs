using adminBlazorWebsite.Abstractions;
using Cloud5mins.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace adminBlazorWebsite.Data
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly HttpClient _httpClient;
        private readonly UrlShortenerConfiguration _configuration;

        public UrlShortenerService(HttpClient httpClient, IOptions<UrlShortenerConfiguration> configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
        }

        public async Task<ShortUrlList> GetUrlList()
        {
            var url = GetFunctionUrl("UrlList");

            CancellationToken cancellationToken = new CancellationToken();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var results = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await System.Text.Json.JsonSerializer.DeserializeAsync<ShortUrlList>(results).ConfigureAwait(false);
        }

        public async Task<ShortUrlList> CreateShortUrl(ShortUrlRequest shortUrlRequest)
        {
            return await SendAsync<ShortUrlList, ShortUrlRequest>(shortUrlRequest, "UrlShortener", HttpMethod.Post).ConfigureAwait(false);
        }

        public async Task<ShortUrlEntity> UpdateShortUrl(ShortUrlEntity editedUrl)
        {
            return await SendAsync(editedUrl, "UrlUpdate", HttpMethod.Post).ConfigureAwait(false);
        }

        public async Task<ShortUrlEntity> ArchiveShortUrl(ShortUrlEntity archivedUrl)
        {
            return await SendAsync(archivedUrl, "UrlArchive", HttpMethod.Post).ConfigureAwait(false);
        }

        private async Task<TOut> SendAsync<TOut, TIn>(TIn entity, string functionName, HttpMethod method)
        {
            var url = GetFunctionUrl(functionName);

            CancellationToken cancellationToken = new CancellationToken();

            using var request = new HttpRequestMessage(method, url);
            using var httpContent = CreateHttpContent(entity);
            request.Content = httpContent;

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            var results = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await System.Text.Json.JsonSerializer.DeserializeAsync<TOut>(results).ConfigureAwait(false);
        }

        private async Task<T> SendAsync<T>(T entity, string functionName, HttpMethod method)
        {
            return await SendAsync<T, T>(entity, functionName, method).ConfigureAwait(false);
        }

        private string GetFunctionUrl(string functionName)
        {
            var funcUrl = new StringBuilder();
            funcUrl.Append(functionName);

            var code = this._configuration.Code;
            if (string.IsNullOrWhiteSpace(code)) return funcUrl.ToString();

            funcUrl.Append("?code=");
            funcUrl.Append(code);

            return funcUrl.ToString();
        }

        private static StringContent CreateHttpContent(object content)
        {
            if (content == null) return null;

            var jsonString = System.Text.Json.JsonSerializer.Serialize(content);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            return httpContent;
        }
    }
}
