using Shortener.Azure.Pocos;

namespace Shortener.AzureServices
{
    public interface IStorageTableHelper
    {
        Task<ShortUrlPoco?> GetShortUrlEntityAsync(ShortUrlPoco row);
        Task<List<ShortUrlPoco>> GetAllShortUrlEntitiesAsync(bool includeArchived);
        Task<ShortUrlPoco?> GetShortUrlEntityByVanityAsync(string vanity, string? partitionKey = null);

        Task<bool> IfShortUrlEntityExistAsync(ShortUrlPoco row);
        Task<bool> IfShortUrlEntityExistByVanityAsync(string vanity, string? partitionKey = null);

        Task<ShortUrlPoco?> ArchiveShortUrlEntityAsync(ShortUrlPoco urlEntity);

        Task<ShortUrlPoco?> SaveShortUrlEntityAsync(ShortUrlPoco newShortUrl);


        Task<int> GetNextTableIdForAuthorityAsync(string authority);


        Task<List<ClickStatsPoco>> GetAllStatsByVanityAsync(string? vanity);
        Task<List<ClickStatsPoco>> GetAllStatsAsync();
        Task SaveClickStatsEntityAsync(ClickStatsPoco newStats);


        Task<string> GetWellKnownContentAsync(string filename);
    }
}