namespace Shortener.AzureServices.Entities
{
    public class ClickStatsEntity : AbstractTableEntity
    {
        public string? Datetime { get; set; }

        public string? Domain { get; set; }

        public ClickStatsEntity() : base("", Guid.NewGuid().ToString()) { }

        public ClickStatsEntity(string vanity, string? domain, string? datetime = null) : base(vanity, Guid.NewGuid().ToString())
        {
            PartitionKey = vanity;
            RowKey = Guid.NewGuid().ToString();
            Domain = domain;
            Datetime = datetime;
        }
    }
}
