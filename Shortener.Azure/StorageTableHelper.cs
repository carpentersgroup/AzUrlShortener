using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Shortener.Azure.Entities;
using Shortener.Azure.Extensions;

namespace Shortener.Azure
{
    public class StorageTableHelper : IStorageTableHelper
    {
        private const string NEXT_ID_PARTITION_KEY = "KEY";

        private readonly CloudTableClient _cloudTableClient;
        private readonly ILogger<StorageTableHelper> _logger;

        public StorageTableHelper(CloudTableClient cloudTableClient, ILogger<StorageTableHelper> logger)
        {
            _cloudTableClient = cloudTableClient;
            _logger = logger;
            TryCreateTables();
        }

        public bool TryCreateTables()
        {
            try
            {
                GetStatsTable().CreateIfNotExists();
                GetUrlsTable().CreateIfNotExists();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tables");
                return false;
            }
        }

        private CloudTable GetStatsTable()
        {
            const string TableName = "ClickStats";
            var table = GetTable(TableName);
            return table;
        }

        private CloudTable GetUrlsTable()
        {
            const string TableName = "UrlsDetails";
            var table = GetTable(TableName);
            return table;
        }

        private CloudTable GetWellKnownTable()
        {
            const string TableName = "WellKnown";
            CloudTable table = GetTable(TableName);
            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetTable(string tableName)
        {
            var table = _cloudTableClient.GetTableReference(tableName);
            return table;
        }

        public async Task<string> GetWellKnownContentAsync(string filename)
        {
            TableOperation selOperation = TableOperation.Retrieve<WellKnownEntity>("WellKnown", filename);
            TableResult result = await GetWellKnownTable().ExecuteAsync(selOperation).ConfigureAwait(false);
            WellKnownEntity? wellKnown = result.Result as WellKnownEntity;
            return wellKnown?.Content ?? "";
        }

        public async Task<ShortUrlEntity?> GetShortUrlEntityAsync(ShortUrlEntity row)
        {
            TableOperation selOperation = TableOperation.Retrieve<ShortUrlEntity>(row.PartitionKey, row.RowKey);
            TableResult result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);
            ShortUrlEntity? eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async Task<List<ShortUrlEntity>> GetAllShortUrlEntitiesAsync(bool includeArchived)
        {
            var tblUrls = GetUrlsTable();

            string filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "KEY");
            filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, "KEY"));

            if (!includeArchived)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterConditionForBool("Archived", QueryComparisons.NotEqual, true));
            }

            // Retrieving all entities that are NOT the NextId entity 
            // (it's the only one in the partition "KEY")
            var rangeQuery = new TableQuery<ShortUrlEntity>().Where(filter);

            var lstShortUrl = await SegmentedQueryAsync(tblUrls, rangeQuery).ConfigureAwait(false);

            return lstShortUrl;
        }

        private static async Task<List<T>> SegmentedQueryAsync<T>(CloudTable table, TableQuery<T> query) where T : TableEntity, new()
        {
            List<T> results = new List<T>();

            TableQuerySegment<T> result = await table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);

            results.AddRange(result);

            while (result.ContinuationToken != null)
            {
                result = await table.ExecuteQuerySegmentedAsync(query, result.ContinuationToken).ConfigureAwait(false);

                results.AddRange(result);
            }

            return results;
        }

        private static async Task<T?> SegmentedQuerySingleAsync<T>(CloudTable table, TableQuery<T> query) where T : TableEntity, new()
        {
            var queryResult = await table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            return queryResult.Results.FirstOrDefault();
        }

        public async Task<List<ClickStatsEntity>> GetAllStatsByVanityAsync(string? vanity)
        {
            if(string.IsNullOrEmpty(vanity))
            {
                return await GetAllStatsAsync().ConfigureAwait(false);
            }

            var tblUrls = GetStatsTable();
            TableContinuationToken? token = null;
            var lstShortUrl = new List<ClickStatsEntity>();
            
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vanity);
            TableQuery<ClickStatsEntity> rangeQuery = new TableQuery<ClickStatsEntity>().Where(filter);

            do
            {
                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token).ConfigureAwait(false);

                lstShortUrl.AddRange(queryResult.Results);

                token = queryResult.ContinuationToken;
            } while (token != null);

            return lstShortUrl;
        }

        public async Task<List<ClickStatsEntity>> GetAllStatsAsync()
        {
            var tblUrls = GetStatsTable();
            TableContinuationToken? token = null;
            var lstShortUrl = new List<ClickStatsEntity>();
            TableQuery<ClickStatsEntity> rangeQuery = new TableQuery<ClickStatsEntity>();

            do
            {
                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token).ConfigureAwait(false);

                lstShortUrl.AddRange(queryResult.Results);

                token = queryResult.ContinuationToken;
            } while (token != null);

            return lstShortUrl;
        }

        /// <summary>
        /// Returns the ShortUrlEntity of the <paramref name="vanity"/>
        /// </summary>
        /// <param name="vanity"></param>
        /// <returns>ShortUrlEntity</returns>
        public async Task<ShortUrlEntity?> GetShortUrlEntityByVanityAsync(string vanity, string? partitionKey = null)
        {
            var tblUrls = GetUrlsTable();
            string filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, vanity);
            if (partitionKey is not null)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            TableQuery<ShortUrlEntity> query = new TableQuery<ShortUrlEntity>().Where(
                            filter: filter);
            return await SegmentedQuerySingleAsync(tblUrls, query).ConfigureAwait(false);
        }

        public async Task<bool> IfShortUrlEntityExistByVanityAsync(string vanity, string? partitionKey = null)
        {
            ShortUrlEntity? shortUrlEntity = await GetShortUrlEntityByVanityAsync(vanity, partitionKey).ConfigureAwait(false);
            return shortUrlEntity != null;
        }

        public async Task<bool> IfShortUrlEntityExistAsync(ShortUrlEntity row)
        {
            var eShortUrl = await GetShortUrlEntityAsync(row).ConfigureAwait(false);
            return eShortUrl != null;
        }

        public async Task<ShortUrlEntity?> ArchiveShortUrlEntityAsync(ShortUrlEntity urlEntity)
        {
            if(urlEntity is null)
            {
                return null;
            }

            var originalUrl = await GetShortUrlEntityAsync(urlEntity).ConfigureAwait(false);
            if (originalUrl is not null)
            {
                originalUrl.IsArchived = true;

                return await SaveShortUrlEntityAsync(originalUrl).ConfigureAwait(false);
            }
            else
            {
                return null;
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
        public async Task SaveShortUrlEntitiesAsync(IEnumerable<ShortUrlEntity> newShortUrls)
        {
            foreach (var batch in newShortUrls.Batch(100))
            {
                TableBatchOperation tableOperations = new TableBatchOperation();
                foreach (var shortUrl in batch)
                {
                    tableOperations.InsertOrReplace(shortUrl);
                }
                _ = await GetUrlsTable().ExecuteBatchAsync(tableOperations).ConfigureAwait(false);
            }
        }

        public async Task<ShortUrlEntity?> SaveShortUrlEntityAsync(ShortUrlEntity newShortUrl)
        {
            TableOperation insOperation = TableOperation.InsertOrMerge(newShortUrl);
            TableResult result = await GetUrlsTable().ExecuteAsync(insOperation).ConfigureAwait(false);
            ShortUrlEntity? eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async Task SaveClickStatsEntityAsync(ClickStatsEntity newStats)
        {
            var insOperation = TableOperation.InsertOrMerge(newStats);
            await GetStatsTable().ExecuteAsync(insOperation).ConfigureAwait(false);
        }

        public async Task<int> GetNextTableIdForAuthorityAsync(string authority)
        {
            string tableKey = authority.SanitiseForTableKey();

            //Get current ID
            var selOperation = TableOperation.Retrieve<NextId>(NEXT_ID_PARTITION_KEY, tableKey);
            var result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);

            if (result.Result is not NextId entity)
            {
                entity = new NextId
                {
                    PartitionKey = NEXT_ID_PARTITION_KEY,
                    RowKey = tableKey,
                    Id = 1024
                };
            }
            entity.Id++;

            //Update
            var updOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation.
            await GetUrlsTable().ExecuteAsync(updOperation).ConfigureAwait(false);

            return entity.Id;
        }

        public async Task MigrateNextTableIdForAuthorityAsync(string authority)
        {
            string tableKey = authority.SanitiseForTableKey();

            //Get current ID
            var selOperation = TableOperation.Retrieve<NextId>("1", NEXT_ID_PARTITION_KEY);
            var result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);

            var newEntity = new NextId
            {
                PartitionKey = NEXT_ID_PARTITION_KEY,
                RowKey = tableKey,
                Id = 1024
            };

            if (result.Result is NextId entity)
            {
                newEntity.Id = entity.Id;
            }

            //Update
            var updOperation = TableOperation.InsertOrMerge(newEntity);

            // Execute the operation.
            await GetUrlsTable().ExecuteAsync(updOperation).ConfigureAwait(false);
        }

        public async Task RemoveShortUrlsByVersion(int versionNumber)
        {
            var tblUrls = GetUrlsTable();
            TableContinuationToken? token = null;
            var result = new List<ShortUrlEntity>();

            string filter = TableQuery.GenerateFilterConditionForInt("Version", QueryComparisons.Equal, versionNumber);
            TableQuery<ShortUrlEntity> rangeQuery = new TableQuery<ShortUrlEntity>().Where(filter);

            do
            {
                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token).ConfigureAwait(false);

                result.AddRange(queryResult.Results);

                token = queryResult.ContinuationToken;
            } while (token != null);

            var partitionGroups = result.GroupBy(x => x.PartitionKey);

            foreach (var partitionGroup in partitionGroups)
            {
                foreach (var batch in partitionGroup.Batch(100))
                {
                    TableBatchOperation tableOperations = new TableBatchOperation();
                    foreach (var shortUrl in batch)
                    {
                        tableOperations.Delete(shortUrl);
                    }
                    _ = await GetUrlsTable().ExecuteBatchAsync(tableOperations).ConfigureAwait(false);
                }
            }
        }
    }
}
