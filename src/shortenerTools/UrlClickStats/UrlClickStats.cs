using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cloud5mins.Function
{
    public class UrlClickStats : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlClickStats(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlClickStats")]
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

            try
            {
                var result = new ClickStatsEntityList
                {
                    ClickStatsList = await _storageTableHelper.GetAllStatsByVanity(clickStatsRequest.Vanity)
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
