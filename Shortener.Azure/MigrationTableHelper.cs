using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Shortener.AzureServices.Entities;
using Shortener.AzureServices.Extensions;

namespace Shortener.AzureServices
{
    public interface IMigrationTableHelper
    {
        Task<List<ShortUrlEntity>> GetAllShortUrlEntitiesAsync();
        Task MigrateNextTableIdForAuthorityAsync(string authority);
        Task RemoveShortUrlsByVersion(int versionNumber);
        Task SaveShortUrlEntitiesCrossPartitionAsync(IEnumerable<ShortUrlEntity> newShortUrls);
    }

    public class MigrationTableHelper : IMigrationTableHelper
    {
        private const string NEXT_ID_PARTITION_KEY = "KEY";

        private const string URLSDETAILS_TABLE_NAME = "UrlsDetails";

        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<StorageTableHelper> _logger;

        public MigrationTableHelper(TableServiceClient cloudTableClient, ILogger<StorageTableHelper> logger)
        {
            _tableServiceClient = cloudTableClient;
            _logger = logger;
        }

        private TableClient GetUrlsTable() => GetTable(URLSDETAILS_TABLE_NAME);

        private TableClient GetTable(string tableName) => _tableServiceClient.GetTableClient(tableName);

        public async Task MigrateNextTableIdForAuthorityAsync(string authority)
        {
            try
            {
                string tableKey = authority.SanitiseForTableKey();

                //Get current ID
                TableClient tableClient = GetUrlsTable();
                var result = await tableClient.GetEntityAsync<NextIdEntity>("1", NEXT_ID_PARTITION_KEY).ConfigureAwait(false);

                var newEntity = new NextIdEntity
                {
                    PartitionKey = NEXT_ID_PARTITION_KEY,
                    RowKey = tableKey,
                    Id = 1024
                };

                if (result.Value is NextIdEntity entity)
                {
                    newEntity.Id = entity.Id;
                }

                await tableClient.UpsertEntityAsync(newEntity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error migrating next table id for authority {authority}", authority);
            }
        }

        public async Task<List<ShortUrlEntity>> GetAllShortUrlEntitiesAsync()
        {
            try
            {
                var tblUrls = GetUrlsTable();

                string filter = "PartitionKey ne 'KEY' and RowKey ne 'KEY'";

                var results = tblUrls.QueryAsync<ShortUrlEntity>(filter);

                List<ShortUrlEntity> entities = new List<ShortUrlEntity>();
                await foreach (var result in results.ConfigureAwait(false))
                {
                    entities.Add(result);
                }
                return entities;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting all short url entities");
                return new List<ShortUrlEntity>();
            }
        }

        /// <summary>
        /// Saves a batch of ShortUrlEntity to table storage across multiple partitions
        /// </summary>
        /// <param name="newShortUrls"></param>
        /// <returns></returns>
        public async Task SaveShortUrlEntitiesCrossPartitionAsync(IEnumerable<ShortUrlEntity> newShortUrls)
        {
            var partitionGroups = newShortUrls.GroupBy(x => x.PartitionKey);

            foreach (var partitionGroup in partitionGroups)
            {
                await SaveShortUrlEntitiesAsync(partitionGroup).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Saves a batch of ShortUrlEntity to table storage
        /// 
        /// All entities must have the same partition key
        /// </summary>
        /// <param name="newShortUrls"></param>
        /// <returns></returns>
        private async Task SaveShortUrlEntitiesAsync(IEnumerable<ShortUrlEntity> newShortUrls)
        {
            var table = GetUrlsTable();
            foreach (var batch in newShortUrls.Batch(100))
            {
                // Create the batch.
                List<TableTransactionAction> addEntitiesBatch = new List<TableTransactionAction>();

                // Add the entities to be added to the batch.
                addEntitiesBatch.AddRange(batch.Select(e => new TableTransactionAction(TableTransactionActionType.Add, e)));

                try
                {
                    // Submit the batch.
                    _ = await table.SubmitTransactionAsync(addEntitiesBatch).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error saving short url entities");
                }
            }
        }

        /// <summary>
        /// Deletes a batch of ShortUrlEntity to table storage across multiple partitions
        /// </summary>
        /// <param name="newShortUrls"></param>
        /// <returns></returns>
        private async Task DeleteEntitiesCrossPartitionAsync(IEnumerable<ShortUrlEntity> newShortUrls)
        {
            var partitionGroups = newShortUrls.GroupBy(x => x.PartitionKey);

            foreach (var partitionGroup in partitionGroups)
            {
                await DeleteEntitiesAsync(partitionGroup).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes a batch of ShortUrlEntity to table storage
        /// 
        /// All entities must have the same partition key
        /// </summary>
        /// <param name="newShortUrls"></param>
        /// <returns></returns>
        private async Task DeleteEntitiesAsync(IEnumerable<ShortUrlEntity> newShortUrls)
        {
            var table = GetUrlsTable();
            foreach (var batch in newShortUrls.Batch(100))
            {
                // Create the batch.
                List<TableTransactionAction> addEntitiesBatch = new List<TableTransactionAction>();

                // Add the entities to be added to the batch.
                addEntitiesBatch.AddRange(batch.Select(e => new TableTransactionAction(TableTransactionActionType.Delete, e)));

                // Submit the batch.
                _ = await table.SubmitTransactionAsync(addEntitiesBatch).ConfigureAwait(false);
            }
        }

        public async Task RemoveShortUrlsByVersion(int versionNumber)
        {
            try
            {
                var tblUrls = GetUrlsTable();

                var results = tblUrls.QueryAsync<ShortUrlEntity>(s => s.Version == versionNumber);

                List<ShortUrlEntity> entities = new List<ShortUrlEntity>();
                await foreach (var result in results.ConfigureAwait(false))
                {
                    entities.Add(result);
                }
                await DeleteEntitiesCrossPartitionAsync(entities).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error removing short urls by version {versionNumber}", versionNumber);
            }
        }
    }
}
