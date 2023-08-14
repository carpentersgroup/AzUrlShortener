using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.Azure;
using Shortener.Azure.Entities;
using Shortener.Core.Configuration;
using Shortener.Azure.Extensions;
using System.Net;

namespace Shortener.Core.Redirect
{
    public class UrlRedirectService : IUrlRedirectService
    {
        private readonly IUserIpLocationService _userIpLocationService;
        private readonly UrlShortenerConfiguration _configuration;
        private readonly IStorageTableHelper _storageTableHelper;
        private readonly ILogger<UrlRedirectService> _logger;

        public UrlRedirectService(
            IUserIpLocationService userIpLocationService,
            IOptions<UrlShortenerConfiguration> configuration,
            IStorageTableHelper storageTableHelper,
            ILogger<UrlRedirectService> logger)
        {
            _userIpLocationService = userIpLocationService;
            _configuration = configuration.Value;
            _storageTableHelper = storageTableHelper;
            _logger = logger;
        }

        public async Task<Result<string, RedirectStatus>> ProcessAsync(IPAddress? ipAddress, string host, string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
            {
                _logger.LogInformation("Bad Link, resorting to fallback.");

                return new Result<string, RedirectStatus>
                {
                    Message = "Bad Link",
                    Value = _configuration.DefaultRedirectUrl,
                    Status = RedirectStatus.Invalid
                };
            }
            ShortUrlEntity? newUrl;

            string hostPartitionKey = host.SanitiseForTableKey();
            newUrl = await _storageTableHelper.GetShortUrlEntityByVanityAsync(shortUrl, hostPartitionKey).ConfigureAwait(false);
            //TODO: Remove this once all links have been migrated to the new format
            if (newUrl is null)
            {
                var tempUrl = new ShortUrlEntity(shortUrl.First().ToString(), string.Empty, shortUrl);
                newUrl = await _storageTableHelper.GetShortUrlEntityAsync(tempUrl).ConfigureAwait(false);
            }

            if (newUrl is not null)
            {
                var redirectUrl = newUrl.ActiveUrl;
                _logger.LogInformation("Found it: {Url}", redirectUrl);

                await SetUrlClickStatsAsync(newUrl, ipAddress).ConfigureAwait(false);

                ClickStatsEntity newStats = new ClickStatsEntity(newUrl.RowKey, host);
                List<Task> tasks = new List<Task>
                {
                    _storageTableHelper.SaveClickStatsEntityAsync(newStats),
                    _storageTableHelper.SaveShortUrlEntityAsync(newUrl)
                };

                await Task.WhenAll(tasks).ConfigureAwait(false);

                return new Result<string, RedirectStatus>
                {
                    Value = redirectUrl,
                    Status = RedirectStatus.Success
                };
            }
            else
            {
                _logger.LogInformation("Could not find link, resorting to fallback.");
                return new Result<string, RedirectStatus>
                {
                    Message = "Short Url not found",
                    Value = _configuration.DefaultRedirectUrl,
                    Status = RedirectStatus.NotFound
                };
            }
        }

        private async Task SetUrlClickStatsAsync(ShortUrlEntity newUrl, IPAddress? ip)
        {
            newUrl.Clicks++;

            if(!_configuration.RecordCountryStats)
            {
                return;
            }

            UserIpResponse? userIpResponse;
            try
            {
                if (ip is null)
                {
                    _logger.LogInformation("Failed to get client ip");
                    return;
                }
                else
                {
                    _logger.LogInformation("Got client ip");
                }                    

                userIpResponse = await _userIpLocationService.GetUserIpAsync(ip.ToString(), CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                return;
            }

            if (newUrl.ClicksByCountry is null)
            {
                _logger.LogInformation("New click, new country, creating with click count to 1");
                newUrl.ClicksByCountry = new Dictionary<string, int>() { { userIpResponse.CountryName, 1 } };
            }
            else if (newUrl.ClicksByCountry.ContainsKey(userIpResponse.CountryName))
            {
                _logger.LogInformation("Previously seen country, increasing click count");
                newUrl.ClicksByCountry[userIpResponse.CountryName] = newUrl.ClicksByCountry[userIpResponse.CountryName] + 1;
            }
            else
            {
                _logger.LogInformation("New country, settings click count to 1");
                newUrl.ClicksByCountry.Add(userIpResponse.CountryName, 1);
            }
        }
    }
}
