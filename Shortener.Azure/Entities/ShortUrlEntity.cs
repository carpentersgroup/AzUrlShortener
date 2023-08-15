namespace Shortener.AzureServices.Entities
{
    public class ShortUrlEntity : AbstractTableEntity
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public string? ShortUrl { get; set; }

        public int Clicks { get; set; }

        public string ClicksByCountry { get; set; }

        public bool? IsArchived { get; set; }

        public string? SchedulesPropertyRaw { get; set; }

        public int Version { get; set; } = 1;

        public int Algorithm { get; set; }

        public ShortUrlEntity() : base("", "")
        {
            Url = "";
            Title = "";
            ClicksByCountry = "";
        }

        public ShortUrlEntity(string partitionKey, string longUrl, string endUrl, string title = "", string? schedules = null) : base(partitionKey, endUrl)
        {
            Url = longUrl.Trim();
            Title = title.Trim();
            ClicksByCountry = "";
            IsArchived = false;
            SchedulesPropertyRaw = schedules;
        }
    }
}
