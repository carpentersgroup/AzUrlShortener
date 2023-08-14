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
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Cloud5mins.domain;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using shortenerTools.Abstractions;
using Microsoft.Extensions.Options;

namespace Cloud5mins.Function
{
    public class UrlClickStatsByDay : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        private readonly UrlShortenerConfiguration _configuration;

        public UrlClickStatsByDay(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration)
        {
            this._storageTableHelper = storageTableHelper;
            this._configuration = configuration.Value;
        }

        [FunctionName("UrlClickStatsByDay")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var authResult = ValidateAuth(principal, log);

            if (authResult is not null)
            {
                return authResult;
            }

            var clickStatsRequest = await ParseRequestAsync<UrlClickStatsRequest>(req);

            if (clickStatsRequest is null)
            {
                return new BadRequestResult();
            }

            var result = new ClickDateList();

            try
            {

                var rawStats = await this._storageTableHelper.GetAllStatsByVanity(clickStatsRequest.Vanity);
                

                result.Items = rawStats.GroupBy( s => DateTime.Parse(s.Datetime).Date)
                                            .Select(stat => new ClickDate{
                                                DateClicked = stat.Key.ToString("yyyy-MM-dd"),
                                                Count = stat.Count()
                                            }).OrderBy(s => DateTime.Parse(s.DateClicked).Date).ToList<ClickDate>();

                var host = this._configuration.UseCustomDomain ? this._configuration.CustomDomain : req.RequestUri.GetLeftPart(UriPartial.Authority);

                result.Url = Utility.GetShortUrl(host, clickStatsRequest.Vanity);
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
