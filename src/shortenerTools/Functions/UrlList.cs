/*
```c#
Input:


Output:
    {
        "Url": "https://SOME_URL",
        "Clicks": 0,
        "PartitionKey": "d",
        "title": "Quickstart: Create your first function in Azure using Visual Studio"
        "RowKey": "doc",
        "Timestamp": "0001-01-01T00:00:00+00:00",
        "ETag": "W/\"datetime'2020-05-06T14%3A33%3A51.2639969Z'\""
    }
*/

using Cloud5mins.domain;
using Fizzibly.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.AzureServices;
using Shortener.Core.Configuration;
using ShortenerTools.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    public class UrlList : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlList(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlList")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] Microsoft.AspNetCore.Http.HttpRequest req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var invalidResult = await HandleAuth(principal, req).ConfigureAwait(false);

            if (invalidResult != null)
            {
                return invalidResult;
            }

            var result = new ListResponse();

            try
            {
                result.UrlList = await _storageTableHelper.GetAllShortUrlEntitiesAsync(includeArchived: false).ConfigureAwait(false);

                string host;
                if (_configuration.UseCustomDomain)
                {
                    host = GetBaseUrlFromUri(_configuration.CustomDomain);
                }
                else
                {
                    host = GetUrlFromRequest(req);
                }

                foreach (var shortUrl in result.UrlList.Where(u => string.IsNullOrWhiteSpace(u.ShortUrl)))
                {
                    shortUrl.ShortUrl = Utility.GetShortUrl(host, shortUrl.Vanity);
                }

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed due to an unexpected error: {errorMessage}.",
                    ex.GetBaseException().Message);

                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
        }
    }
}
