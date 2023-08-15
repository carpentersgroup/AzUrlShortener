using Shortener.Azure.Pocos;
using Shortener.AzureServices.Entities;

namespace Shortener.Azure.Mappers
{
    public static class NextIdEntityMapper
    {
        public static NextIdEntity ToEntity(this NextIdPoco poco)
        {
            return new NextIdEntity
            {
                PartitionKey = "KEY",
                RowKey = poco.Authority,
                Id = poco.Id
            };
        }

        public static NextIdPoco FromEntity(this NextIdEntity entity)
        {
            return new NextIdPoco(entity.RowKey, entity.Id);
        }
    }
}