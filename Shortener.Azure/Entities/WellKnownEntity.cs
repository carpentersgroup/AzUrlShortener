using Microsoft.Azure.Cosmos.Table;

namespace Shortener.Azure.Entities
{
    public class WellKnownEntity : TableEntity
    {
        public WellKnownEntity() 
        {
            Content = "";
        }

        public WellKnownEntity(string filename)
        {
            PartitionKey = "WellKnown";
            RowKey = filename;
            Content = "";
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
