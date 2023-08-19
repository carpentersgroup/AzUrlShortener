/*
```c#
Input:

    {
        // [Required] the end of the URL that you want statistics for.
        "vanity": "azFunc"
    }

Output:
    {
    "items": [
        {
        "dateClicked": "2020-12-19",
        "count": 1
        },
        {
        "dateClicked": "2020-12-03",
        "count": 2
        }
    ],
    "url": ""https://c5m.ca/29"
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using Cloud5mins.domain;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ShortenerTools.Abstractions;
using Microsoft.Extensions.Options;
using Shortener.AzureServices;
using Shortener.Core.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Globalization;
using Fizzibly.Auth.Handlers;

namespace ShortenerTools.Functions
{
    public class UrlClickStatsByDay : FunctionBase
    {
        private const string CLICK_DATE_FORMAT = "yyyy-MM-dd";

        private readonly IStorageTableHelper _storageTableHelper;

        public UrlClickStatsByDay(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlClickStatsByDay")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] UrlClickStatsRequest clickStatsRequest, Microsoft.AspNetCore.Http.HttpRequest req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var invalidResult = await HandleAuth(principal, req).ConfigureAwait(false);

            if (invalidResult != null)
            {
                return invalidResult;
            }

            var result = new ClickDateList();

            try
            {
                System.Collections.Generic.List<Shortener.Azure.Pocos.ClickStatsPoco> rawStats = await _storageTableHelper.GetAllStatsByVanityAsync(clickStatsRequest.Vanity).ConfigureAwait(false);

                result.Items = rawStats.Where(s => s.Datetime != null)
                                        .GroupBy(s => DateOnly.ParseExact(s.Datetime!, Shortener.AzureServices.Entities.Constants.CLICK_STATS_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None))
                                            .Select(stat => new ClickDate
                                            {
                                                DateClicked = stat.Key.ToString(CLICK_DATE_FORMAT),
                                                DateForOrdering = stat.Key,
                                                Count = stat.Count()
                                            }).OrderBy(s => s.DateForOrdering);

                string host = _configuration.UseCustomDomain ? GetBaseUrlFromUri(_configuration.CustomDomain) : GetUrlFromRequest(req);

                result.Url = Utility.GetShortUrl(rawStats.FirstOrDefault()?.Domain ?? host, clickStatsRequest.Vanity ?? "*");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            return new OkObjectResult(result);
        }
    }
}
