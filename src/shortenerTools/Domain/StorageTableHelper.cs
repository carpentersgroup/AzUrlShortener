using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cloud5mins.domain
{
    public class StorageTableHelper : IStorageTableHelper
    {
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
            catch (System.Exception ex)
            { 
                this._logger.LogError(ex, "Error creating tables");
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

        public async Task<string> GetWellKnownContent(string filename)
        {
            TableOperation selOperation = TableOperation.Retrieve<WellKnownEntity>("WellKnown", filename);
            TableResult result = await GetWellKnownTable().ExecuteAsync(selOperation).ConfigureAwait(false);
            WellKnownEntity wellKnown = result.Result as WellKnownEntity;
            return wellKnown.Content;
        }

        public async Task<ShortUrlEntity> GetShortUrlEntity(ShortUrlEntity row)
        {
            var selOperation = TableOperation.Retrieve<ShortUrlEntity>(row.PartitionKey, row.RowKey);
            var result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);
            var eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async Task<List<ShortUrlEntity>> GetAllShortUrlEntities(bool includeArchived)
        {
            var tblUrls = GetUrlsTable();

            string filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "KEY");

            if(!includeArchived)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterConditionForBool("Archived", QueryComparisons.NotEqual, true));
            }

            // Retrieving all entities that are NOT the NextId entity 
            // (it's the only one in the partition "KEY")
            var rangeQuery = new TableQuery<ShortUrlEntity>().Where(filter);

            var lstShortUrl = await SegmentedQueryAsync<ShortUrlEntity>(tblUrls, rangeQuery).ConfigureAwait(false);

            return lstShortUrl;
        }

        private async Task<List<T>> SegmentedQueryAsync<T>(CloudTable table, TableQuery<T> query) where T : TableEntity, new()
        {
            List<T> results = new List<T>();

            TableQuerySegment<T> result = await table.ExecuteQuerySegmentedAsync<T>(query, null).ConfigureAwait(false);

            results.AddRange(result);

            while (result.ContinuationToken != null)
            {
                result = await table.ExecuteQuerySegmentedAsync<T>(query, result.ContinuationToken).ConfigureAwait(false);

                results.AddRange(result);
            }

            return results;
        }

        private static async Task<T> SegmentedQuerySingleAsync<T>(CloudTable table, TableQuery<T> query) where T : TableEntity, new()
        {
            var queryResult = await table.ExecuteQuerySegmentedAsync(query, null).ConfigureAwait(false);
            return queryResult.Results.FirstOrDefault();
        }

        public async Task<List<ClickStatsEntity>> GetAllStatsByVanity(string vanity)
        {
            var tblUrls = GetStatsTable();
            TableContinuationToken token = null;
            var lstShortUrl = new List<ClickStatsEntity>();
            TableQuery<ClickStatsEntity> rangeQuery = new TableQuery<ClickStatsEntity>();

            if (!string.IsNullOrEmpty(vanity))
            {
                rangeQuery = rangeQuery.Where(
                filter: TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vanity));
            }

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
        public async Task<ShortUrlEntity> GetShortUrlEntityByVanity(string vanity, string partitionKey = null)
        {
            var tblUrls = GetUrlsTable();
            string filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, vanity);
            if(partitionKey is not null)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            TableQuery<ShortUrlEntity> query = new TableQuery<ShortUrlEntity>().Where(
                            filter: filter);
            return await SegmentedQuerySingleAsync(tblUrls, query).ConfigureAwait(false);
        }

        public async Task<bool> IfShortUrlEntityExistByVanity(string vanity, string partitionKey = null)
        {
            ShortUrlEntity shortUrlEntity = await GetShortUrlEntityByVanity(vanity, partitionKey).ConfigureAwait(false);
            return shortUrlEntity != null;
        }

        public async Task<bool> IfShortUrlEntityExist(ShortUrlEntity row)
        {
            var eShortUrl = await GetShortUrlEntity(row).ConfigureAwait(false);
            return eShortUrl != null;
        }

        public async Task<ShortUrlEntity> ArchiveShortUrlEntity(ShortUrlEntity urlEntity)
        {
            var originalUrl = await GetShortUrlEntity(urlEntity).ConfigureAwait(false);
            originalUrl.IsArchived = true;

            return await SaveShortUrlEntity(originalUrl).ConfigureAwait(false);
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
                foreach(var row in partitionGroup)
                {
                    await SaveShortUrlEntitiesAsync(newShortUrls).ConfigureAwait(false);
                }
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
            TableBatchOperation tableOperations = new TableBatchOperation();
            foreach (var shortUrl in newShortUrls)
            {
                tableOperations.InsertOrMerge(shortUrl);
            }
            _ = await GetUrlsTable().ExecuteBatchAsync(tableOperations).ConfigureAwait(false);
        }

        public async Task<ShortUrlEntity> SaveShortUrlEntity(ShortUrlEntity newShortUrl)
        {
            var insOperation = TableOperation.InsertOrMerge(newShortUrl);
            var result = await GetUrlsTable().ExecuteAsync(insOperation).ConfigureAwait(false);
            var eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async void SaveClickStatsEntity(ClickStatsEntity newStats)
        {
            var insOperation = TableOperation.InsertOrMerge(newStats);
            await GetStatsTable().ExecuteAsync(insOperation).ConfigureAwait(false);
        }

        public async Task<int> GetNextTableId()
        {
            //Get current ID
            var selOperation = TableOperation.Retrieve<NextId>("1", "KEY");
            var result = await GetUrlsTable().ExecuteAsync(selOperation).ConfigureAwait(false);

            if (!(result.Result is NextId entity))
            {
                entity = new NextId
                {
                    PartitionKey = "1",
                    RowKey = "KEY",
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
    }
}
