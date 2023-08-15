using Shortener.Azure.Pocos;
using Shortener.AzureServices.Entities;

namespace Shortener.Azure.Mappers
{
    public static class ShortUrlEntityMapper
    {
        public static ShortUrlEntity ToEntity(this ShortUrlPoco poco)
        {
            string clicksByCountry = System.Text.Json.JsonSerializer.Serialize(poco.ClicksByCountry) ?? "";

            string? schedules = null;
            if (poco.Schedules is not null)
            {
                System.Text.Json.JsonSerializer.Serialize(poco.Schedules);
            }

            return new ShortUrlEntity()
            {
                PartitionKey = poco.Authority,
                RowKey = poco.Vanity,
                Title = poco.Title,
                Url = poco.Url,
                ClicksByCountry = clicksByCountry,
                ShortUrl = poco.ShortUrl,
                Clicks = poco.Clicks,
                IsArchived = poco.IsArchived,
                SchedulesPropertyRaw = schedules,
                Version = poco.Version,
                Algorithm = poco.Algorithm
            };
        }

        public static IEnumerable<ShortUrlEntity> ToEntity(this IEnumerable<ShortUrlPoco>? entity)
        {
            if (entity is null)
            {
                return Enumerable.Empty<ShortUrlEntity>();
            }

            return entity.Select(ToEntity);
        }

        public static ShortUrlPoco FromEntity(this ShortUrlEntity entity)
        {
            Dictionary<string, int>? clicksByCountry = null;
            if (!string.IsNullOrEmpty(entity.ClicksByCountry))
            {
                clicksByCountry = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(entity.ClicksByCountry);
            }

            clicksByCountry ??= new Dictionary<string, int>();

            Schedule[]? schedules = null;
            if (!string.IsNullOrEmpty(entity.SchedulesPropertyRaw))
            {
                schedules = System.Text.Json.JsonSerializer.Deserialize<Schedule[]>(entity.SchedulesPropertyRaw);
            }

            return new ShortUrlPoco(entity.Title, entity.Url, entity.RowKey)
            {
                Authority = entity.PartitionKey,
                ClicksByCountry = clicksByCountry,
                ShortUrl = entity.ShortUrl,
                Clicks = entity.Clicks,
                IsArchived = entity.IsArchived,
                Schedules = schedules,
                Version = entity.Version,
                Algorithm = entity.Algorithm
            };
        }

        public static IEnumerable<ShortUrlPoco> FromEntity(this IEnumerable<ShortUrlEntity> entity)
        {
            if (entity is null)
            {
                return Enumerable.Empty<ShortUrlPoco>();
            }

            return entity.Select(FromEntity);
        }
    }
}
