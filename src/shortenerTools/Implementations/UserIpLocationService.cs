using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using shortenerTools.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace shortenerTools.Implementations
{
    public class UserIpLocationService : IUserIpLocationService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<UserIpLocationService> logger;

        public UserIpLocationService(HttpClient httpClient, ILogger<UserIpLocationService> logger)
        {
            _httpClient = httpClient;

            this.logger = logger;
        }

        public async Task<UserIpResponse> GetUserIpAsync(string ip, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"/json/{ip}", cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var userip = await System.Text.Json.JsonSerializer.DeserializeAsync<UserIpResponse>(stream).ConfigureAwait(false);

                    logger.LogInformation($"Hit from {userip.CountryName}");

                    return userip;
                }
            }
            else
            {
                logger.LogWarning("Could not get user ip info");

                return new UserIpResponse
                {
                    CountryName = "Unknown"
                };
            }
        }
    }
}