using Azure;
using Azure.Data.Tables;

namespace Shortener.AzureServices.Entities
{
    public abstract class AbstractTableEntity: ITableEntity
    {
        protected AbstractTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
