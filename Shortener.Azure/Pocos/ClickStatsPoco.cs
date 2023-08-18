using Shortener.AzureServices.Entities;

namespace Shortener.Azure.Pocos
{
    public class ClickStatsPoco
    {
        public string? Datetime { get; set; }

        public string Vanity { get; private set; }

        [Obsolete("For backward compatibility only. Use Vanity instead.")]
        public string PartitionKey => Vanity;

        public string? Domain { get; set; }

        public ClickStatsPoco()
        {
            Vanity = "";
        }

        public ClickStatsPoco(string vanity, string domain)
        {
            Vanity = vanity;
            Domain = domain;
            Datetime = DateTime.UtcNow.ToString(Constants.CLICK_STATS_DATE_FORMAT);
        }
    }
}
