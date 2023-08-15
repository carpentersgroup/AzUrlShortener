using Microsoft.Extensions.Logging;
using Shortener.Azure;
using Shortener.Azure.Entities;
using Shortener.Azure.Extensions;
using Shortener.Core.Shorten.Algorithms;

namespace Shortener.Core.Shorten
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly IStorageTableHelper _storageTableHelper;
        private readonly IUrlGenerator _shortUrlGenerator;
        private readonly ILogger<UrlShortenerService> _logger;

        public UrlShortenerService(
            IStorageTableHelper storageTableHelper,
            IUrlGenerator shortUrlGenerator,
            ILogger<UrlShortenerService> logger)
        {
            _storageTableHelper = storageTableHelper;
            _shortUrlGenerator = shortUrlGenerator;
            _logger = logger;
        }

        public async Task<Result<ShortResponse, ShortnerStatus>> ShortenUrl(ShortRequest shortRequest)
        {
            if (shortRequest is null)
            {
                return new Result<ShortResponse, ShortnerStatus>
                {
                    Status = ShortnerStatus.InvalidRequest,
                    Value = null,
                    Message = "No url shortening request was supplied."
                };
            }

            if (shortRequest.Url is null)
            {
                return new Result<ShortResponse, ShortnerStatus>
                {
                    Status = ShortnerStatus.InvalidUrl,
                    Value = null,
                    Message = "No url was supplied."
                };
            }

            if (shortRequest.Host is null)
            {
                return new Result<ShortResponse, ShortnerStatus>
                {
                    Status = ShortnerStatus.UnknownError,
                    Value = null,
                    Message = "No host was supplied."
                };
            }

            var title = string.IsNullOrWhiteSpace(shortRequest.Title) ? "" : shortRequest.Title.Trim();

            bool hasVanitySupplied = !string.IsNullOrWhiteSpace(shortRequest.Vanity);

            ShortenerAlgorithm algorithm = ShortenerAlgorithm.None;
            if(hasVanitySupplied)
            {
                shortRequest.Vanity = shortRequest.Vanity!.Trim();

                if (await _storageTableHelper.IfShortUrlEntityExistByVanityAsync(shortRequest.Vanity, shortRequest.Host.Authority.SanitiseForTableKey()).ConfigureAwait(false))
                {
                    return new Result<ShortResponse, ShortnerStatus>
                    {
                        Status = ShortnerStatus.Conflict,
                        Value = null,
                        Message = "This Short URL already exist."
                    };
                }
            }
            else
            {
                var generateResult = await _shortUrlGenerator.GenerateAsync(shortRequest.Host.Authority).ConfigureAwait(false);
                algorithm = generateResult.Algorithm;
                shortRequest.Vanity = generateResult.Vanity;
            }

            ShortUrlEntity newRow = new ShortUrlEntity(shortRequest.Host.Authority.SanitiseForTableKey(), shortRequest.Url.ToString(), shortRequest.Vanity, title, shortRequest.Schedules);
            newRow.Version = 1;
            newRow.Algorithm = (int)algorithm;
            
            string hostUrl = shortRequest.Host.ToString();
            
            _logger.LogInformation("-> host = {hostUrl}", hostUrl);

            newRow.ShortUrl = string.Concat(hostUrl.TrimEnd('/'), "/", newRow.RowKey);
            
            await _storageTableHelper.SaveShortUrlEntityAsync(newRow).ConfigureAwait(false);

            

            var result = new ShortResponse(hostUrl, newRow.Url, newRow.RowKey, newRow.Title);

            _logger.LogInformation("Short Url created.");

            return new Result<ShortResponse, ShortnerStatus>
            {
                Status = ShortnerStatus.Success,
                Value = result
            };
        }
    }
}
