using Microsoft.Azure.Cosmos.Table;
using Shortener.Azure.Extensions;

namespace Shortener.Azure.Entities
{
    public class ShortUrlEntity : TableEntity
    {
        public string Url { get; set; }

        private string? _activeUrl { get; set; }

        /// <summary>
        /// TODO: This should be an extension method
        /// </summary>
        public string? ActiveUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_activeUrl))
                    _activeUrl = GetActiveUrl();
                return _activeUrl;
            }
        }

        public string Title { get; set; }

        public string? ShortUrl { get; set; }

        public int Clicks { get; set; }

        [EntityJsonPropertyConverter]
        public Dictionary<string, int> ClicksByCountry { get; set; }

        public bool? IsArchived { get; set; }

        [EntityJsonPropertyConverter]
        public Schedule[]? SchedulesPropertyRaw { get; set; }

        [IgnoreProperty]
        public Schedule[]? Schedules => SchedulesPropertyRaw;

        public int Version { get; set; } = 0;

        public int Algorithm { get; set; }

        public ShortUrlEntity() 
        {
            PartitionKey = "";
            RowKey = "";
            Url = "";
            Title = "";
            ClicksByCountry = new Dictionary<string, int>();
        }

        public ShortUrlEntity(string partitionKey, string longUrl, string endUrl, string title = "", Schedule[]? schedules = null)
        {
            PartitionKey = partitionKey;
            RowKey = endUrl;
            Url = longUrl.Trim();
            Title = title.Trim();
            ClicksByCountry = new Dictionary<string, int>();
            IsArchived = false;
            SchedulesPropertyRaw = schedules;
        }

        private string GetActiveUrl()
        {
            if (Schedules != null)
                return GetActiveUrl(DateTime.UtcNow, Schedules);

            return Url;
        }

        private string GetActiveUrl(DateTime pointInTime, Schedule[] schedules)
        {
            var active = schedules.Where(s =>
                s.End > pointInTime && //hasn't ended
                s.Start < pointInTime //already started
                ).OrderBy(s => s.Start); //order by start to process first link

            var activeLink = active.FirstOrDefault(a => a.IsActive(pointInTime));

            return activeLink?.AlternativeUrl ?? Url;
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            EntityJsonPropertyConverter.Serialize(this, results);
            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            EntityJsonPropertyConverter.Deserialize(this, properties);
        }
    }
}
