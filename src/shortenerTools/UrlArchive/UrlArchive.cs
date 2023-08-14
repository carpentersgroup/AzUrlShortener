/*
```c#
Input:
    {
         // [Required]
        "PartitionKey": "d",

         // [Required]
        "RowKey": "doc",

    }


*/

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
    public class UrlArchive : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlArchive(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlArchive")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var authResult = ValidateAuth(principal, log);

            if(authResult is not null)
            {
                return authResult;
            }

            var shortUrlEntity = await ParseRequestAsync<ShortUrlEntity>(req);

            if (shortUrlEntity is null)
            {
                return new BadRequestResult();
            }

            try
            {
                shortUrlEntity = await _storageTableHelper.ArchiveShortUrlEntity(shortUrlEntity);
                return new OkObjectResult(shortUrlEntity);
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
