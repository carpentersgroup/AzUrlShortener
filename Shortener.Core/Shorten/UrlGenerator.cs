using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shortener.Azure;
using Shortener.Core.Configuration;
using Shortener.Core.Shorten.Algorithms;

namespace Shortener.Core.Shorten
{
    public class UrlGenerator : IUrlGenerator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStorageTableHelper _storageTableHelper;
        private readonly UrlShortenerConfiguration _options;

        public UrlGenerator(IServiceProvider serviceProvider, IOptions<UrlShortenerConfiguration> options, IStorageTableHelper storageTableHelper)
        {
            _serviceProvider = serviceProvider;
            _storageTableHelper = storageTableHelper;
            _options = options.Value;
        }

        public async Task<(string Vanity, ShortenerAlgorithm Algorithm)> GenerateAsync(string? host)
        {
            if (_options.DefaultAlgorithm == ShortenerAlgorithm.IdPlusRandomFixedLength
                || _options.DefaultAlgorithm == ShortenerAlgorithm.EncodeDecode)
            {
                if (string.IsNullOrWhiteSpace(host))
                {
                    throw new ArgumentNullException(nameof(host));
                }
            }

            switch (_options.DefaultAlgorithm)
            {
                case ShortenerAlgorithm.IdPlusRandomFixedLength:
                    {
                        var hasher = this._serviceProvider.GetRequiredService<IHash>();
                        var endUrl = await GetValidEndUrlFixedLength(_storageTableHelper, hasher).ConfigureAwait(false);
                        int id = await this._storageTableHelper.GetNextTableIdForAuthorityAsync(host!).ConfigureAwait(false);

                        return (id + endUrl, _options.DefaultAlgorithm);
                    }
                    break;
                case ShortenerAlgorithm.RandomFixedLength:
                    {
                        var hasher = this._serviceProvider.GetRequiredService<IHash>();
                        var endUrl =  await GetValidEndUrlFixedLength(_storageTableHelper, hasher).ConfigureAwait(false);
                        return (endUrl, _options.DefaultAlgorithm);
                    }
                    break;
                case ShortenerAlgorithm.RandomExtendableLength:
                    {
                        var hasher = this._serviceProvider.GetRequiredService<IHash>();
                        var endUrl = await GetValidEndUrl(_storageTableHelper, hasher).ConfigureAwait(false);
                        return (endUrl, _options.DefaultAlgorithm);
                    }
                    break;
                case ShortenerAlgorithm.EncodeDecode:
                    {
                        var encoder = this._serviceProvider.GetRequiredService<IEncode>();
                        int id = await this._storageTableHelper.GetNextTableIdForAuthorityAsync(host!).ConfigureAwait(false);
                        var endUrl = encoder.Encode(id);
                        return (endUrl, _options.DefaultAlgorithm);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_options.DefaultAlgorithm));
            }
        }

        private static async Task<string> GetValidEndUrlFixedLength(IStorageTableHelper stgHelper, IHash shortUrlGenerator)
        {
            const int MIN_LENGTH = 7;
            string code = shortUrlGenerator.Generate(MIN_LENGTH);
            if (await stgHelper.IfShortUrlEntityExistByVanityAsync(code).ConfigureAwait(false))
            {
                return await GetValidEndUrl(stgHelper, shortUrlGenerator, MIN_LENGTH).ConfigureAwait(false);
            }

            return code;
        }

        private static async Task<string> GetValidEndUrl(IStorageTableHelper stgHelper, IHash shortUrlGenerator, int collisionCounter = 0, int length = 5)
        {
            if (collisionCounter > 5)
            {
                length++;
                collisionCounter = 0;
            }

            string code = shortUrlGenerator.Generate(length);
            if (await stgHelper.IfShortUrlEntityExistByVanityAsync(code).ConfigureAwait(false))
            {
                return await GetValidEndUrl(stgHelper, shortUrlGenerator, collisionCounter + 1, length).ConfigureAwait(false);
            }

            return code;
        }
    }
}
