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
    public class UrlListMigration : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        private readonly UrlShortenerConfiguration _configuration;

        public UrlListMigration(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration)
        {
            _storageTableHelper = storageTableHelper;
            this._configuration = configuration.Value;
        }

        [FunctionName("UrlListMigration")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "get", Route = null)] HttpRequestMessage req,
        ILogger log,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            try
            {
                var shortUrls = await _storageTableHelper.GetAllShortUrlEntities(includeArchived: true);
                shortUrls = shortUrls.Where(p => !(p.IsArchived ?? false)).ToList();

                const string PARTITION_KEY = "ShortUrlPartition";
                foreach (var shortUrl in shortUrls)
                {
                    shortUrl.PartitionKey = PARTITION_KEY;
                    shortUrl.IsArchived = false;
                }

                await _storageTableHelper.SaveShortUrlEntitiesAsync(shortUrls);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed due to an unexpected error: {errorMessage}.",
                    ex.GetBaseException().Message);

                return new UnprocessableEntityObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.UnprocessableEntity
                });
            }
        }
    }
}
