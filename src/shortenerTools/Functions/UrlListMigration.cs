using Cloud5mins.domain;
using Fizzibly.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shortener.Azure;
using Shortener.Azure.Extensions;
using Shortener.Core.Configuration;
using Shortener.Core.Shorten.Algorithms;
using ShortenerTools.Abstractions;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    public class UrlListMigration : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlListMigration(IStorageTableHelper storageTableHelper, IOptions<UrlShortenerConfiguration> configuration, HandlerContainer authHandlerContainer) : base(configuration, authHandlerContainer)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlListMigration")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "get", Route = null)] Microsoft.AspNetCore.Http.HttpRequest req,
        ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                string authority = _configuration.UseCustomDomain ? _configuration.CustomDomain.Authority : GetAuthorityFromRequest(req);

                await _storageTableHelper.MigrateNextTableIdForAuthorityAsync(authority).ConfigureAwait(false);
                var shortUrls = await _storageTableHelper.GetAllShortUrlEntitiesAsync(includeArchived: true).ConfigureAwait(false);
                sw.Reset();
                log.LogInformation($"Retrieved {shortUrls.Count} short urls in {sw.ElapsedMilliseconds} ms.");
                long totalMilliseconds = sw.ElapsedMilliseconds;
                sw.Start();
                foreach (var shortUrl in shortUrls)
                {
                    shortUrl.IsArchived ??= false;
                    shortUrl.Version = 0;
                    if (shortUrl.Algorithm == (int)ShortenerAlgorithm.None)
                    {
                        shortUrl.Algorithm = (int)ShortenerAlgorithm.IdPlusRandomFixedLength;
                    }
                }
                await _storageTableHelper.SaveShortUrlEntitiesCrossPartitionAsync(shortUrls).ConfigureAwait(false);

                sw.Reset();
                log.LogInformation($"Updated {shortUrls.Count} short urls in {sw.ElapsedMilliseconds} ms.");
                totalMilliseconds += sw.ElapsedMilliseconds;
                sw.Start();

                string hostPartitionKey = authority.SanitiseForTableKey();
                string baseUrl = _configuration.UseCustomDomain ? GetBaseUrlFromUri(_configuration.CustomDomain) : GetUrlFromRequest(req);
                foreach (var shortUrl in shortUrls)
                {
                    shortUrl.PartitionKey = hostPartitionKey;
                    shortUrl.IsArchived ??= false;
                    shortUrl.Version = 1;
                    shortUrl.Algorithm = (int)_configuration.DefaultAlgorithm;
                    shortUrl.ShortUrl = Utility.GetShortUrl(baseUrl, shortUrl.RowKey); 
                }

                sw.Reset();
                log.LogInformation($"Updated {shortUrls.Count} short urls in {sw.ElapsedMilliseconds} ms.");
                totalMilliseconds += sw.ElapsedMilliseconds;
                sw.Start();

                await _storageTableHelper.SaveShortUrlEntitiesCrossPartitionAsync(shortUrls).ConfigureAwait(false);

                sw.Stop();
                log.LogInformation($"Saved {shortUrls.Count} short urls in {sw.ElapsedMilliseconds} ms.");
                totalMilliseconds += sw.ElapsedMilliseconds;
                log.LogInformation($"Migration completed in {totalMilliseconds} ms.");

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
