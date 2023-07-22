namespace Cloud5mins.Config
{
    public class UrlShortenerConfiguration
    {
        public bool EnableApiAccess { get; set; }
        public string UrlShortenApiRoleName { get; set; }
        public string CustomDomain { get; set; }
        public string Code { get; set; }
        public bool UseCustomDomain => !string.IsNullOrWhiteSpace(CustomDomain);
    }
}
