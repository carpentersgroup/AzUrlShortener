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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using shortenerTools.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cloud5mins.Function
{
    public class UrlList : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        private readonly UrlShortenerConfiguration _configuration;

        public UrlList(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration)
        {
            _storageTableHelper = storageTableHelper;
            this._configuration = configuration.Value;
        }

        [FunctionName("UrlList")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var invalidResult = ValidateRequest(context, req, principal, log);

            if (invalidResult is not null)
            {
                return invalidResult;
            }

            var result = new ListResponse();

            try
            {
                result.UrlList = await _storageTableHelper.GetAllShortUrlEntities();
                result.UrlList = result.UrlList.Where(p => !(p.IsArchived ?? false)).ToList();

                var host = this._configuration.UseCustomDomain ? this._configuration.CustomDomain : req.RequestUri.GetLeftPart(UriPartial.Authority);
                foreach (var shortUrl in result.UrlList)
                {
                    shortUrl.ShortUrl = Utility.GetShortUrl(host, shortUrl.RowKey);
                }

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "{functionName} failed due to an unexpected error: {errorMessage}.",
                    context.FunctionName, ex.GetBaseException().Message);

                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
        }

        public IActionResult ValidateRequest(ExecutionContext context, HttpRequestMessage req, ClaimsPrincipal principal, ILogger log)
        {
            var invalidRequest = Utility.CheckUserImpersonatedAuth(principal, log);
            if (invalidRequest != null)
            {
                return invalidRequest;
            }

            LogAuthenticatedUser(principal, context, log);

            return null;
        }
    }
}
