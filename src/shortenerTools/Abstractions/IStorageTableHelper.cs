using Cloud5mins.domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace shortenerTools.Abstractions
{
    public interface IStorageTableHelper
    {
        Task<ShortUrlEntity> GetShortUrlEntity(ShortUrlEntity row);
        Task<List<ShortUrlEntity>> GetAllShortUrlEntities(bool includeArchived);
        Task<List<ClickStatsEntity>> GetAllStatsByVanity(string vanity);
        Task<bool> IfShortUrlEntityExist(ShortUrlEntity row);
        Task<ShortUrlEntity> ArchiveShortUrlEntity(ShortUrlEntity urlEntity);
        Task<ShortUrlEntity> SaveShortUrlEntity(ShortUrlEntity newShortUrl);
        Task SaveShortUrlEntitiesAsync(IEnumerable<ShortUrlEntity> newShortUrls);
        Task SaveShortUrlEntitiesCrossPartitionAsync(IEnumerable<ShortUrlEntity> newShortUrls);
        void SaveClickStatsEntity(ClickStatsEntity newStats);
        Task<int> GetNextTableId();
        Task<bool> IfShortUrlEntityExistByVanity(string vanity, string partitionKey = null);
        Task<string> GetWellKnownContent(string filename);
        Task<ShortUrlEntity> GetShortUrlEntityByVanity(string vanity, string partitionKey = null);
    }
}