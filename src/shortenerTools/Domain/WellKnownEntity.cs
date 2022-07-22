using Microsoft.Azure.Cosmos.Table;

namespace Cloud5mins.domain
{
    public class WellKnownEntity : TableEntity
    {
        public WellKnownEntity() { }

        public WellKnownEntity(string filename)
        {
            PartitionKey = "WellKnown";
            RowKey = filename;
        }

        public WellKnownEntity(string filename, string content)
        {
            PartitionKey = "WellKnown";
            RowKey = filename;
            Content = content;
        }

        public string Content { get; set; }
    }


}
