/*
```c#
Input:

    {
        // [Required] The url you wish to have a short version for
        "url": "https://docs.microsoft.com/en-ca/azure/azure-functions/functions-create-your-first-function-visual-studio",
        
        // [Optional] Title of the page, or text description of your choice.
        "title": "Quickstart: Create your first function in Azure using Visual Studio"

        // [Optional] the end of the URL. If nothing one will be generated for you.
        "vanity": "azFunc"

        // [Optional] A schedule for the url to be active. If nothing the url will be active right away.
        "schedules":
        [
            {
                "start": "2020-05-06T14:33:51.2639969Z",
                "end": "2020-05-06T14:33:51.2639969Z",
                "alternativeUrl": "https://docs.microsoft.com/en-ca/azure/azure-functions/functions-create-your-first-function-visual-studio",
                "cron": "0 0 0 0 0",    
                "durationMinutes": 0
            }
        ]

        // [Optional] the custom domain that will be able to redirect to this url. If nothing the default custom domain will be used or the azure function url if no custom domain is used.
        "host": "http://c5m.ca"
    }

Output:
    {
        "ShortUrl": "http://c5m.ca/azFunc",
        "LongUrl": "https://docs.microsoft.com/en-ca/azure/azure-functions/functions-create-your-first-function-visual-studio"
    }
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShortenerTools.Abstractions;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shortener.Core;
using System.Web.Http;
using Shortener.Core.Configuration;
using Shortener.Core.Shorten;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Fizzibly.Auth;

namespace ShortenerTools.Functions
{
    public class UrlShortener : FunctionBase
    {
        private readonly IUrlShortenerService _urlShortenerService;

        public UrlShortener(
            IUrlShortenerService urlShortenerService,
            IOptions<UrlShortenerConfiguration> options,
            HandlerContainer authHandlerContainer) : base(options, authHandlerContainer)
        {
            _urlShortenerService = urlShortenerService;
        }

        [FunctionName("UrlShortener")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] ShortRequest shortRequest,
        Microsoft.AspNetCore.Http.HttpRequest req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            return await RunUrlShortenerAsync(shortRequest, req, log, principal).ConfigureAwait(false);
        }

        private async Task<IActionResult> RunUrlShortenerAsync(ShortRequest shortRequest, HttpRequest req, ILogger log, ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var invalidResult = await HandleAuth(principal, req).ConfigureAwait(false);

            if (invalidResult != null)
            {
                return invalidResult;
            }

            invalidResult = Validate(shortRequest);

            if (invalidResult != null)
            {
                return invalidResult;
            }

            try
            {
                if (shortRequest.Host is null)
                {
                    if (_configuration.UseCustomDomain)
                    {
                        shortRequest.Host = _configuration.CustomDomain;
                    }
                    else
                    {
                        var uri = new Uri(GetUrlFromRequest(req));
                        shortRequest.Host = uri;
                    }
                }

                var result = await _urlShortenerService.ShortenUrl(shortRequest).ConfigureAwait(false);

                return result switch
                {
                    { Status: ShortnerStatus.Success } => new OkObjectResult(result.Value),
                    { Status: ShortnerStatus.Conflict } => new ConflictObjectResult(result.Message),
                    { Status: ShortnerStatus.InvalidVanity } => new BadRequestObjectResult(result.Message),
                    { Status: ShortnerStatus.InvalidUrl } => new BadRequestObjectResult(result.Message),
                    { Status: ShortnerStatus.InvalidTitle } => new BadRequestObjectResult(result.Message),
                    { Status: ShortnerStatus.InvalidRequest } => new BadRequestObjectResult(result.Message),
                    _ => new InternalServerErrorResult(),
                };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "failed due to an unexpected error: {errorMessage}.",
                     ex.GetBaseException().Message);

                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
        }

        public static IActionResult? Validate(ShortRequest shortRequest)
        {
            if (shortRequest == null)
            {
                return new BadRequestResult();
            }

            // If the Url parameter only contains whitespaces or is empty return with BadRequest.
            if (shortRequest.Url is null)
            {
                return new BadRequestObjectResult("The url parameter can not be empty.");
            }

            // Validates if input.url is a valid absolute url, aka is a complete reference to the resource, ex: http(s)://google.com
            if (!shortRequest.Url.IsAbsoluteUri)
            {
                return new BadRequestObjectResult($"{shortRequest.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'https://'.");
            }

            return null;
        }
    }
}
