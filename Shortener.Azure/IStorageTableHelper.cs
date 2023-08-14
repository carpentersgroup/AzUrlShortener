using Shortener.Azure.Entities;

namespace Shortener.Azure
{
    public interface IStorageTableHelper
    {
        Task<ShortUrlEntity?> GetShortUrlEntityAsync(ShortUrlEntity row);
        Task<List<ShortUrlEntity>> GetAllShortUrlEntitiesAsync(bool includeArchived);
        Task<ShortUrlEntity?> GetShortUrlEntityByVanityAsync(string vanity, string? partitionKey = null);

        Task<bool> IfShortUrlEntityExistAsync(ShortUrlEntity row);
        Task<bool> IfShortUrlEntityExistByVanityAsync(string vanity, string? partitionKey = null);

        Task<ShortUrlEntity?> ArchiveShortUrlEntityAsync(ShortUrlEntity urlEntity);

        Task<ShortUrlEntity?> SaveShortUrlEntityAsync(ShortUrlEntity newShortUrl);
        Task SaveShortUrlEntitiesAsync(IEnumerable<ShortUrlEntity> newShortUrls);
        Task SaveShortUrlEntitiesCrossPartitionAsync(IEnumerable<ShortUrlEntity> newShortUrls);

        Task RemoveShortUrlsByVersion(int versionNumber);


        Task MigrateNextTableIdForAuthorityAsync(string authority);
        Task<int> GetNextTableIdForAuthorityAsync(string authority);


        Task<List<ClickStatsEntity>> GetAllStatsByVanityAsync(string? vanity);
        Task<List<ClickStatsEntity>> GetAllStatsAsync();
        Task SaveClickStatsEntityAsync(ClickStatsEntity newStats);


        Task<string> GetWellKnownContentAsync(string filename);
    }
}