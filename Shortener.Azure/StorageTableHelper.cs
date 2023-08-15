using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Shortener.Azure.Mappers;
using Shortener.Azure.Pocos;
using Shortener.AzureServices.Entities;
using Shortener.AzureServices.Extensions;

namespace Shortener.AzureServices
{

    public class StorageTableHelper : IStorageTableHelper
    {
        private const string NEXT_ID_PARTITION_KEY = "KEY";
        private const string WELLKNOWN_PARITION_KEY = "WellKnown";

        private const string WELLKNOWN_TABLE_NAME = "WellKnown";
        private const string CLICKSTATS_TABLE_NAME = "ClickStats";
        private const string URLSDETAILS_TABLE_NAME = "UrlsDetails";

        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<StorageTableHelper> _logger;

        public StorageTableHelper(TableServiceClient cloudTableClient, ILogger<StorageTableHelper> logger)
        {
            _tableServiceClient = cloudTableClient;
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

        private TableClient GetStatsTable() => GetTable(CLICKSTATS_TABLE_NAME);

        private TableClient GetUrlsTable() => GetTable(URLSDETAILS_TABLE_NAME);

        private TableClient GetWellKnownTable()
        {
            _ = _tableServiceClient.CreateTableIfNotExists(WELLKNOWN_TABLE_NAME);
            return GetTable(WELLKNOWN_TABLE_NAME);
        }

        private TableClient GetTable(string tableName) => _tableServiceClient.GetTableClient(tableName);

        public async Task<string> GetWellKnownContentAsync(string filename)
        {
            WellKnownEntity? wellKnown;
            try
            {
                wellKnown = await GetWellKnownTable().GetEntityAsync<WellKnownEntity>(WELLKNOWN_PARITION_KEY, filename).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting well known content");
                return "";
            }

            return wellKnown?.Content ?? "";
        }

        public async Task<ShortUrlPoco?> GetShortUrlEntityAsync(ShortUrlPoco row)
        {
            try
            {
                var entity = await GetUrlsTable().GetEntityAsync<ShortUrlEntity>(row.Authority, row.Vanity).ConfigureAwait(false);
                return entity.Value.FromEntity();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting well known content");
                return null;
            }
        }

        public async Task<List<ShortUrlPoco>> GetAllShortUrlEntitiesAsync(bool includeArchived)
        {
            try
            {
                var tblUrls = GetUrlsTable();

                string filter = "PartitionKey ne 'KEY' and RowKey ne 'KEY'";

                if (!includeArchived)
                {
                    filter = TableClient.CreateQueryFilter<ShortUrlEntity>(s => s.PartitionKey != "KEY" && s.RowKey != "KEY" && s.IsArchived != true);
                }

                var results = tblUrls.QueryAsync<ShortUrlEntity>(filter);

                List<ShortUrlEntity> entities = new List<ShortUrlEntity>();
                await foreach (var result in results.ConfigureAwait(false))
                {
                    entities.Add(result);
                }
                return entities.FromEntity().ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting all short url entities");
                return new List<ShortUrlPoco>();
            }
        }

        public async Task<List<ClickStatsPoco>> GetAllStatsByVanityAsync(string? vanity)
        {
            if(string.IsNullOrEmpty(vanity))
            {
                return await GetAllStatsAsync().ConfigureAwait(false);
            }

            try
            {
                var tblUrls = GetStatsTable();

                string filter = TableClient.CreateQueryFilter<ClickStatsEntity>(s => s.PartitionKey == vanity);

                var results = tblUrls.QueryAsync<ClickStatsEntity>(filter);

                List<ClickStatsEntity> entities = new List<ClickStatsEntity>();
                await foreach (var result in results.ConfigureAwait(false))
                {
                    entities.Add(result);
                }
                return entities.FromEntity().ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting all stats by vanity {vanity}", vanity);
                return new List<ClickStatsPoco>();
            }
        }

        public async Task<List<ClickStatsPoco>> GetAllStatsAsync()
        {
            try
            {
                var tblUrls = GetStatsTable();

                var results = tblUrls.QueryAsync<ClickStatsEntity>();

                List<ClickStatsEntity> entities = new List<ClickStatsEntity>();
                await foreach (var result in results.ConfigureAwait(false))
                {
                    entities.Add(result);
                }
                return entities.FromEntity().ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting all stats");
                return new List<ClickStatsPoco>();
            }
        }

        /// <summary>
        /// Returns the ShortUrlEntity of the <paramref name="vanity"/>
        /// </summary>
        /// <param name="vanity"></param>
        /// <returns>ShortUrlEntity</returns>
        public async Task<ShortUrlPoco?> GetShortUrlEntityByVanityAsync(string vanity, string? partitionKey = null)
        {
            try
            {
                var tblUrls = GetUrlsTable();

                if (partitionKey is not null)
                {
                    var response = await tblUrls.GetEntityAsync<ShortUrlEntity>(partitionKey, vanity).ConfigureAwait(false);
                    return response.Value.FromEntity();
                }
                else
                {
                    string filter = TableClient.CreateQueryFilter<ShortUrlEntity>(s => s.RowKey == vanity);
                    var results = tblUrls.QueryAsync<ShortUrlEntity>(filter, 1);

                    await foreach (var item in results.AsPages().ConfigureAwait(false))
                    {
                        return item.Values.FirstOrDefault()?.FromEntity();
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting all short url entities");
                return null;
            }

            return null;
        }

        public async Task<bool> IfShortUrlEntityExistByVanityAsync(string vanity, string? partitionKey = null)
        {
            ShortUrlPoco? shortUrlEntity = await GetShortUrlEntityByVanityAsync(vanity, partitionKey).ConfigureAwait(false);
            return shortUrlEntity != null;
        }

        public async Task<bool> IfShortUrlEntityExistAsync(ShortUrlPoco row)
        {
            ShortUrlPoco? eShortUrl = await GetShortUrlEntityAsync(row).ConfigureAwait(false);
            return eShortUrl != null;
        }

        public async Task<ShortUrlPoco?> ArchiveShortUrlEntityAsync(ShortUrlPoco urlEntity)
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

        public async Task<ShortUrlPoco?> SaveShortUrlEntityAsync(ShortUrlPoco newShortUrl)
        {
            try
            {
                var table = GetUrlsTable();

                var response = await table.UpsertEntityAsync(newShortUrl.ToEntity()).ConfigureAwait(false);
                if(response.IsError)
                {
                      this._logger.LogError("Error saving short url entity: {ReasonPhrase}", response.ReasonPhrase);
                    return null;
                }
                return newShortUrl;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error saving short url entity");
                return null;
            }
        }

        public async Task SaveClickStatsEntityAsync(ClickStatsPoco newStats)
        {
            try
            {
                var table = GetStatsTable();

                var response = await table.AddEntityAsync(newStats.ToEntity()).ConfigureAwait(false);
                if (response.IsError)
                {
                    this._logger.LogError("Error saving click stats entity: {ReasonPhrase}", response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error saving click stats entity");
            }
        }

        public async Task<int> GetNextTableIdForAuthorityAsync(string authority)
        {
            string tableKey = authority.SanitiseForTableKey();
            TableClient tableClient = GetUrlsTable();

            try
            {
                var response = await tableClient.GetEntityIfExistsAsync<NextIdEntity>(NEXT_ID_PARTITION_KEY, tableKey).ConfigureAwait(false);

                NextIdEntity entity;
                if (!response.HasValue)
                {
                    entity = new NextIdEntity
                    {
                        PartitionKey = NEXT_ID_PARTITION_KEY,
                        RowKey = tableKey,
                        Id = 1024
                    };

                    await tableClient.AddEntityAsync(entity).ConfigureAwait(false);
                }
                else
                {                     
                    entity = response.Value;
                    entity.Id++;
                    await tableClient.UpsertEntityAsync(entity).ConfigureAwait(false);
                }

                return entity.Id;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting all short url entities");
                return 0;
            }
        }
    }
}
