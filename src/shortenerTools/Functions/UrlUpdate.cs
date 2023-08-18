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
using Fizzibly.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.Azure.Pocos;
using Shortener.AzureServices;
using Shortener.Core.Configuration;
using ShortenerTools.Abstractions;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace ShortenerTools.Functions
{
    public class UrlUpdate : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlUpdate(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        //TODO: Convert Entity to DTO
        [FunctionName("UrlUpdate")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] ShortUrlPoco shortUrlEntity, Microsoft.AspNetCore.Http.HttpRequest req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var invalidResult = await HandleAuth(principal, req).ConfigureAwait(false);

            if (invalidResult != null)
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
                return new BadRequestObjectResult($"{shortUrlEntity.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'https://'.");
            }

            try
            {
                string baseUrl = _configuration.UseCustomDomain ? GetBaseUrlFromUri(_configuration.CustomDomain) : GetUrlFromRequest(req);

                log.LogInformation("-> host = {baseUrl}", baseUrl);

                if (string.IsNullOrWhiteSpace(shortUrlEntity.ShortUrl))
                {
                    shortUrlEntity.ShortUrl = Utility.GetShortUrl(baseUrl, shortUrlEntity.Vanity);
                }

                ShortUrlPoco? result = await _storageTableHelper.SaveShortUrlEntityAsync(shortUrlEntity).ConfigureAwait(false);

                if(result is null)
                {
                    return new InternalServerErrorResult();
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
