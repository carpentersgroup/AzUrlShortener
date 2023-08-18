using Microsoft.Extensions.Logging;
using System.Net;

namespace Shortener.Core.Redirect
{
    public class UserIpLocationService : IUserIpLocationService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<UserIpLocationService> _logger;

        public UserIpLocationService(HttpClient httpClient, ILogger<UserIpLocationService> logger)
        {
            _httpClient = httpClient;

            this._logger = logger;
        }

        public async Task<UserIpResponse> GetUserIpAsync(string ip, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"/json/{ip}", cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                UserIpResponse? userip = await System.Text.Json.JsonSerializer.DeserializeAsync<UserIpResponse>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (userip is null)
                {
                    _logger.LogWarning("Failed to read user ip response");

                    return new UserIpResponse
                    {
                        CountryName = "Unknown"
                    };
                }

                _logger.LogInformation("Hit from {CountryName}", userip.CountryName);

                return userip;
            }
            else
            {
                _logger.LogWarning("Could not get user ip info");

                return new UserIpResponse
                {
                    CountryName = "Unknown"
                };
            }
        }

        public IPAddress GetClientIpn(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            string? firstForwardedForHeader = HttpHeaderExtensions.GetFirstHeaderValue(request, "X-Forwarded-For");

            if (string.IsNullOrEmpty(firstForwardedForHeader))
            {
                return GetRemoteAddress(request);
            }

            var ip = GetClientIpn(firstForwardedForHeader);
            if(ip is null)
            {
                return GetRemoteAddress(request);
            }
            else
            {
                return ip;
            }

            static IPAddress GetRemoteAddress(Microsoft.AspNetCore.Http.HttpRequest request)
            {
                return request.HttpContext.Connection.RemoteIpAddress;
            }
        }

        public IPAddress? GetClientIpn(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request)
        {
            string? firstForwardedForHeader = HttpHeaderExtensions.GetFirstHeaderValue(request, "X-Forwarded-For");

            if (string.IsNullOrEmpty(firstForwardedForHeader))
            {
                return GetRemoteAddress(request);
            }

            var ip = GetClientIpn(firstForwardedForHeader);
            if (ip is null)
            {
                return GetRemoteAddress(request);
            }
            else
            {
                return ip;
            }

            static IPAddress? GetRemoteAddress(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request)
            {
                return GetClientIpn(HttpHeaderExtensions.GetFirstHeaderValue(request, "x-azure-clientip"));
            }
        }

        public static IPAddress? GetClientIpn(string? firstForwardedForHeader)
        {
            if(string.IsNullOrWhiteSpace(firstForwardedForHeader))
            {
                return null;
            }

            string[] firstForwardedHeaderValue = firstForwardedForHeader.Split(',');

            string? address = firstForwardedHeaderValue.FirstOrDefault();
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            string? ipn = address.Split(':').FirstOrDefault();
            if (IPAddress.TryParse(ipn, out IPAddress? result))
            {
                return result!;
            }
            else
            {
                return null;
            }
        }
    }
}