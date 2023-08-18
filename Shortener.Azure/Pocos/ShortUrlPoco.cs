using Shortener.AzureServices.Extensions;

namespace Shortener.Azure.Pocos
{
    public class ShortUrlPoco
    {
        public ShortUrlPoco(string authority, string url, string vanity, string title = "", Schedule[]? schedules = null)
        {
            Authority = authority;
            Vanity = vanity;
            Url = url.Trim();
            Title = title.Trim();
            ClicksByCountry = new Dictionary<string, int>();
            IsArchived = false;
            Schedules = schedules;
        }

        public string Authority { get; set; }

        public string Vanity { get; set; }

        public string Url { get; set; }

        private string? _activeUrl { get; set; }

        /// <summary>
        /// TODO: This should be an extension method
        /// </summary>
        public string? ActiveUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_activeUrl))
                    _activeUrl = GetActiveUrl();
                return _activeUrl;
            }
        }

        public string Title { get; set; }

        public string? ShortUrl { get; set; }

        public int Clicks { get; set; }

        public Dictionary<string, int> ClicksByCountry { get; set; }

        public bool? IsArchived { get; set; }

        public Schedule[]? Schedules { get; set; }

        public int Version { get; set; } = 0;

        public int Algorithm { get; set; }

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
    }
}
