using Fizzibly.Auth.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.AzureServices;
using Shortener.Core.Configuration;
using ShortenerTools.Abstractions;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    public class UrlRemoveMigrated : FunctionBase
    {
        private readonly IMigrationTableHelper _storageTableHelper;

        public UrlRemoveMigrated(IMigrationTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlRemoveMigrated")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = null)] Microsoft.AspNetCore.Http.HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                await _storageTableHelper.RemoveShortUrlsByVersion(0).ConfigureAwait(false);

                sw.Stop();
                log.LogInformation($"Removed version 0 short urls in {sw.ElapsedMilliseconds} ms.");

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
