namespace Shortener.AzureServices.Entities
{
    public class NextIdEntity : AbstractTableEntity
    {
        private const string PARTITION_KEY = "KEY";

        public NextIdEntity() : base(PARTITION_KEY, "")
        {
        }

        public NextIdEntity(string partitionKey) : base(partitionKey, "")
        {
        }

        public int Id { get; set; }
    }
}