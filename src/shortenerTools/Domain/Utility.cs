namespace Cloud5mins.domain
{

    public static class Utility
    {
        public static string GetShortUrl(string host, string vanity)
        {
            return host.TrimEnd('/') + "/" + vanity;
        }
    }
}