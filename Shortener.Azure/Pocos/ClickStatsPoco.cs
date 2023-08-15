using Shortener.AzureServices.Entities;

namespace Shortener.Azure.Pocos
{
    public class ClickStatsPoco : AbstractTableEntity
    {
        public string? Datetime { get; set; }

        public string Vanity { get; private set; }

        public string? Domain { get; set; }

        public ClickStatsPoco() : base("", Guid.NewGuid().ToString())
        {
            Vanity = "";
        }

        public ClickStatsPoco(string vanity, string domain) : base(vanity, Guid.NewGuid().ToString())
        {
            Vanity = vanity;
            Domain = domain;
            Datetime = DateTime.UtcNow.ToString(Constants.CLICK_STATS_DATE_FORMAT);
        }
    }
}
