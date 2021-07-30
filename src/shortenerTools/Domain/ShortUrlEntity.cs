using Microsoft.Azure.Cosmos.Table;
using shortenerTools.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace Cloud5mins.domain
{
    public class ShortUrlEntity : TableEntity
    {
        public string Url { get; set; }
        private string _activeUrl { get; set; }

        public string ActiveUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_activeUrl))
                    _activeUrl = GetActiveUrl();
                return _activeUrl;
            }
        }


        public string Title { get; set; }

        public string ShortUrl { get; set; }

        public int Clicks { get; set; }

        [EntityJsonPropertyConverter]
        public Dictionary<string, int> ClicksByCountry { get; set; }

        public bool? IsArchived { get; set; }
        public string SchedulesPropertyRaw { get; set; }

        [IgnoreProperty]
        public Schedule[] Schedules
        {
            get
            {
                if (String.IsNullOrEmpty(SchedulesPropertyRaw))
                    return null;
                return JsonConvert.DeserializeObject<Schedule[]>(SchedulesPropertyRaw);
            }
            set
            {
                SchedulesPropertyRaw = JsonConvert.SerializeObject(value);
            }
        }

        public ShortUrlEntity() { }

        public ShortUrlEntity(string longUrl, string endUrl)
        {
            Initialize(longUrl, endUrl, string.Empty, null);
        }

        public ShortUrlEntity(string longUrl, string endUrl, Schedule[] schedules)
        {
            Initialize(longUrl, endUrl, string.Empty, schedules);
        }

        public ShortUrlEntity(string longUrl, string endUrl, string title, Schedule[] schedules)
        {
            Initialize(longUrl, endUrl, title, schedules);
        }

        private void Initialize(string longUrl, string endUrl, string title, Schedule[] schedules)
        {
            PartitionKey = endUrl.First().ToString();
            RowKey = endUrl;
            Url = longUrl;
            Title = title;
            ClicksByCountry = new Dictionary<string, int>();
            IsArchived = false;
            Schedules = schedules;
        }

        public static ShortUrlEntity GetEntity(string longUrl, string endUrl, string title, Schedule[] schedules)
        {

            return new ShortUrlEntity
            {
                PartitionKey = endUrl.First().ToString(),
                RowKey = endUrl,
                Url = longUrl,
                Title = title,
                Schedules = schedules
            };
        }

        private string GetActiveUrl()
        {
            if (Schedules != null)
                return GetActiveUrl(DateTime.UtcNow);
            return Url;
        }
        private string GetActiveUrl(DateTime pointInTime)
        {
            var link = Url;
            var active = Schedules.Where(s =>
                s.End > pointInTime && //hasn't ended
                s.Start < pointInTime //already started
                ).OrderBy(s => s.Start); //order by start to process first link

            foreach (var sched in active.ToArray())
            {
                if (sched.IsActive(pointInTime))
                {
                    link = sched.AlternativeUrl;
                    break;
                }
            }
            return link;
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
