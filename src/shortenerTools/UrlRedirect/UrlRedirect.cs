using Cloud5mins.domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System.Net;
using System.Net.Http;
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
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlRedirect/{shortUrl}")] HttpRequestMessage req,
            string shortUrl,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

            var redirectUrl = "https://azure.com";

            if (!string.IsNullOrWhiteSpace(shortUrl))
            {
                redirectUrl = _configuration["defaultRedirectUrl"];

                var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);

                var newUrl = await _storageTableHelper.GetShortUrlEntity(tempUrl);

                if (newUrl != null)
                {
                    log.LogInformation($"Found it: {newUrl.Url}");
                    await SetUrlClickStatsAsync(newUrl, req);
                    _storageTableHelper.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey));
                    await _storageTableHelper.SaveShortUrlEntity(newUrl);
                    redirectUrl = WebUtility.UrlDecode(newUrl.Url);
                }
            }
            else
            {
                log.LogInformation("Bad Link, resorting to fallback.");
            }

            var res = req.CreateResponse(HttpStatusCode.Redirect);
            res.Headers.Add("Location", redirectUrl);
            return res;
        }

        private async Task SetUrlClickStatsAsync(ShortUrlEntity newUrl, HttpRequestMessage req)
        {
            newUrl.Clicks++;

            shortenerTools.Models.UserIpResponse userIpResponse = null;
            try
            {
                var ip = Utility.GetClientIpn(req);

                if (ip != null)
                {
                    userIpResponse = await _userIpLocationService.GetUserIpAsync(ip.ToString(), CancellationToken.None);
                }
            }
            catch
            {
                
            }

            if(userIpResponse is null || string.IsNullOrEmpty(userIpResponse.CountryName))
            {
                userIpResponse = new shortenerTools.Models.UserIpResponse
                {
                    CountryName = "Unknown"
                };
            }

            if (newUrl.ClicksByCountry.ContainsKey(userIpResponse.CountryName))
            {
                newUrl.ClicksByCountry[userIpResponse.CountryName] = newUrl.ClicksByCountry[userIpResponse.CountryName]++;
            }
            else
            {
                newUrl.ClicksByCountry.Add(userIpResponse.CountryName, 1);
            }
        }
    }
}
