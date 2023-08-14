namespace Shortener.Azure.Entities
{
    public class ClickStatsEntityList
    {
        public List<ClickStatsEntity> ClickStatsList { get; set; }

        public ClickStatsEntityList() 
        {
            ClickStatsList = new List<ClickStatsEntity>();
        }

        public ClickStatsEntityList(List<ClickStatsEntity> list)
        {
            ClickStatsList = list;
        }
    }
}