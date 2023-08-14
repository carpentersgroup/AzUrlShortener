using System.ComponentModel.DataAnnotations;

namespace Shortener.Azure.Entities
{
    public class ShortUrlDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Url { get; set; } = null!;

        public string? Title { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string? ShortUrl { get; set; }

        public bool? IsArchived { get; set; }

        public Schedule[]? Schedules { get; set; }

        public int Version { get; set; } = 0;

        public int Algorithm { get; set; }
    }
}
