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

using Fizzibly.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.Azure.Pocos;
using Shortener.AzureServices;
using Shortener.AzureServices.Entities;
using Shortener.Core.Configuration;
using ShortenerTools.Abstractions;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    public class UrlArchive : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlArchive(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        //TODO: Replace Entity with a DTO
        [FunctionName("UrlArchive")]
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

            try
            {
                ShortUrlPoco? updatedShortUrlEntity = await _storageTableHelper.ArchiveShortUrlEntityAsync(shortUrlEntity).ConfigureAwait(false);
                if (updatedShortUrlEntity is null)
                {
                    return new NotFoundObjectResult(new
                    {
                        Message = $"ShortUrlEntity to archive not found.",
                        StatusCode = HttpStatusCode.NotFound
                    });
                }
                return new OkObjectResult(updatedShortUrlEntity);
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
