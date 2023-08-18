namespace Shortener.AzureServices.Entities
{
    public class WellKnownEntity : AbstractTableEntity
    {
        public WellKnownEntity() : base("WellKnown", "")
        {
            Content = "";
        }

        public WellKnownEntity(string filename) : base("WellKnown", filename)
        {
            Content = "";
        }

        public WellKnownEntity(string filename, string content) : base("WellKnown", filename)
        {
            Content = content;
        }

        public string Content { get; set; }
    }
}
