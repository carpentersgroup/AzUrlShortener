using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Cloud5mins.Function
{
    public class UrlRedirect
    {
        private readonly IUserIpLocationService _userIpLocationService;
        private readonly IConfiguration _configuration;
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlRedirect(IUserIpLocationService userIpLocationService, IConfiguration configuration, IStorageTableHelper storageTableHelper)
        {
            _userIpLocationService = userIpLocationService;
            _configuration = configuration;
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlRedirect")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlRedirect/{shortUrl}")] Microsoft.AspNetCore.Http.HttpRequest req,
            string shortUrl,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

            string redirectUrl;

            if (!string.IsNullOrWhiteSpace(shortUrl))
            {
                var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);

                var newUrl = await _storageTableHelper.GetShortUrlEntity(tempUrl);

                if (newUrl != null)
                {
                    log.LogInformation($"Found it: {newUrl.Url}");
                    await SetUrlClickStatsAsync(newUrl, req, log);
                    _storageTableHelper.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey));
                    await _storageTableHelper.SaveShortUrlEntity(newUrl);
                    return new RedirectResult(newUrl.Url, false);
                }
            }
            else
            {
                log.LogInformation("Bad Link, resorting to fallback.");
                return new RedirectResult(_configuration["defaultRedirectUrl"], false);
            }
        }

        private async Task SetUrlClickStatsAsync(ShortUrlEntity newUrl, Microsoft.AspNetCore.Http.HttpRequest req, ILogger log)
        {
            newUrl.Clicks++;

            shortenerTools.Models.UserIpResponse userIpResponse = null;
            try
            {
                var ip = Utility.GetClientIpn(req);

                log.LogInformation(ip == null ? "Failed to get client ip" : "Got client ip");

                if (ip is null)
                    return;

                userIpResponse = await _userIpLocationService.GetUserIpAsync(ip.ToString(), CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                return;
            }

            if (newUrl.ClicksByCountry.ContainsKey(userIpResponse.CountryName))
            {
                log.LogInformation("Previously seen country, increasing click count");
                newUrl.ClicksByCountry[userIpResponse.CountryName] = ++newUrl.ClicksByCountry[userIpResponse.CountryName];
            }
            else
            {
                log.LogInformation("New country, settings click count to 1");
                newUrl.ClicksByCountry.Add(userIpResponse.CountryName, 1);
            }
        }
    }
}
