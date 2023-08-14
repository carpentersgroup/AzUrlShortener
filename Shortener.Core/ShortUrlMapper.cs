namespace Shortener.Azure.Entities
{
    public static class ShortUrlMapper
    {         public static ShortUrlDto ToDto(this ShortUrlEntity entity)
        {
            return new ShortUrlDto
            {
                Url = entity.Url,
                Title = entity.Title,
                ShortUrl = entity.ShortUrl,
                IsArchived = entity.IsArchived,
                Schedules = entity.Schedules,
                Version = entity.Version,
                Algorithm = entity.Algorithm
            };
        }

        public static ShortUrlEntity ToEntity(this ShortUrlDto dto, string partitionKey)
        {
            ArgumentNullException.ThrowIfNull(dto.ShortUrl);

            return new ShortUrlEntity(partitionKey, dto.Url, dto.ShortUrl, dto.Title ?? "", dto.Schedules)
            {
                IsArchived = dto.IsArchived,
                Version = dto.Version,
                Algorithm = dto.Algorithm
            };
        }
    }
}
