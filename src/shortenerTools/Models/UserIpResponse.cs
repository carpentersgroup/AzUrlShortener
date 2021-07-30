using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace shortenerTools.Models
{
    [ExcludeFromCodeCoverage]
    public class UserIpResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("ip")]
        public string Ip { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("country_name")]
        public string CountryName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("region_code")]
        public string RegionCode { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("region_name")]
        public string RegionName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("city")]
        public string City { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("zip_code")]
        public string ZipCode { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("time_zone")]
        public string TimeZone { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("latitude")]
        public float Latitude { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("longitude")]
        public float Longitude { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("metro_code")]
        public int MetroCode { get; set; }
    }
}