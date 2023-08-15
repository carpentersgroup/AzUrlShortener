using Shortener.Azure.Pocos;
using Shortener.AzureServices.Entities;

namespace Shortener.Azure.Mappers
{
    public static class WellKnownEntityMapper
    {
        public static WellKnownEntity ToEntity(this WellKnownPoco wellKnown) => new WellKnownEntity(wellKnown.Filename, wellKnown.Content);

        public static WellKnownPoco FromEntity(this WellKnownEntity wellKnownEntity) => new WellKnownPoco(wellKnownEntity.RowKey, wellKnownEntity.Content);
    }
}
