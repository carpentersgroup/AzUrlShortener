using Shortener.Azure.Pocos;
using Shortener.AzureServices.Entities;

namespace Shortener.Azure.Mappers
{
    public static class ClickStatsEntityMapper
    {
        public static ClickStatsEntity ToEntity(this ClickStatsPoco poco)
        {
            return new ClickStatsEntity(poco.Vanity, poco.Domain, poco.Datetime);
        }

        public static IEnumerable<ClickStatsEntity> ToEntity(this IEnumerable<ClickStatsPoco> entity)
        {
            return entity.Select(ToEntity);
        }

        public static ClickStatsPoco FromEntity(this ClickStatsEntity entity)
        {
            return new ClickStatsPoco(entity.PartitionKey, entity.Domain ?? "")
            {
                Datetime = entity.Datetime
            };
        }

        public static IEnumerable<ClickStatsPoco> FromEntity(this IEnumerable<ClickStatsEntity> entity)
        {
            return entity.Select(FromEntity);
        }
    }
}
