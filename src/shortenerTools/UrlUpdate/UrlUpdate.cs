/*
```c#
Input:
    {
         // [Required]
        "PartitionKey": "d",

         // [Required]
        "RowKey": "doc",

        // [Optional] New Title for this URL, or text description of your choice.
        "title": "Quickstart: Create your first function in Azure using Visual Studio"

        // [Optional] New long Url where the the user will be redirect
        "Url": "https://SOME_URL"
    }


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
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cloud5mins.Function
{
    public class UrlUpdate : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        private readonly UrlShortenerConfiguration _configuration;

        public UrlUpdate(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration)
        {
            _storageTableHelper = storageTableHelper;
            this._configuration = configuration.Value;
        }

        [FunctionName("UrlUpdate")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            // Validation of the inputs
            var (requestValid, invalidResult, shortUrlEntity) = await ValidateRequestAsync<ShortUrlEntity>(context, req, principal, log);

            if (!requestValid)
            {
                return invalidResult;
            }

            // If the Url parameter only contains whitespaces or is empty return with BadRequest.
            if (string.IsNullOrWhiteSpace(shortUrlEntity.Url))
            {
                return new BadRequestObjectResult("The url parameter can not be empty.");
            }

            // Validates if input.url is a valid absolute url, aka is a complete reference to the resource, ex: http(s)://google.com
            if (!Uri.IsWellFormedUriString(shortUrlEntity.Url, UriKind.Absolute))
            {
                return new BadRequestObjectResult($"{shortUrlEntity.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'.");
            }

            try
            {
                var result = await _storageTableHelper.SaveShortUrlEntity(shortUrlEntity);
                
                string customDomain = this._configuration.CustomDomain;

                var host = string.IsNullOrEmpty(customDomain) ? req.RequestUri.GetLeftPart(UriPartial.Authority) : customDomain;

                log.LogInformation($"-> host = {host}");

                result.ShortUrl = Utility.GetShortUrl(host, result.RowKey);

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
    }
}
