using Cloud5mins.domain;
using Fizzibly.Auth.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.AzureServices;
using Shortener.Core;
using Shortener.Core.Configuration;
using ShortenerTools.Abstractions;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    public class UrlClickStats : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlClickStats(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlClickStats")]
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

            try
            {
                var result = new ClickStatsPocoList
                {
                    ClickStatsList = await _storageTableHelper.GetAllStatsByVanityAsync(clickStatsRequest.Vanity).ConfigureAwait(false)
                };

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
