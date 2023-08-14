using Microsoft.Azure.Cosmos.Table;

namespace Shortener.Azure.Entities
{
    public class ClickStatsEntity : TableEntity
    {
        public string? Datetime { get; set; }

        public string? Domain { get; set; }

        public ClickStatsEntity() { }

        public ClickStatsEntity(string vanity, string domain)
        {
            PartitionKey = vanity;
            RowKey = Guid.NewGuid().ToString();
            Domain = domain;
            Datetime = DateTime.UtcNow.ToString(Constants.CLICK_STATS_DATE_FORMAT);
        }
    }
}
