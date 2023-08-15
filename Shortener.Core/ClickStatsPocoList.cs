using Shortener.Azure.Pocos;
using Shortener.AzureServices.Entities;

namespace Shortener.Core
{
    public class ClickStatsPocoList
    {
        public List<ClickStatsPoco> ClickStatsList { get; set; }

        public ClickStatsPocoList()
        {
            ClickStatsList = new List<ClickStatsPoco>();
        }

        public ClickStatsPocoList(List<ClickStatsPoco> list)
        {
            ClickStatsList = list;
        }
    }
}