namespace Fizzibly.Auth
{
    public class JwtSettings
    {
        public const string KEY = "JwtSettings";

        public List<string>? AllowedClients { get; set; }

        public List<string> AllowedTenants { get; set; } = new List<string>();

        public string MetadataAddress { get; set; } = string.Empty;

        public List<string> RequiredRoles { get; set; } = new List<string>();
    }
}