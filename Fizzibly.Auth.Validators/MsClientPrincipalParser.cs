using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Fizzibly.Auth.Validators
{
    public static class MsClientPrincipalParser
    {

        //public static ClaimsPrincipal? Parse(Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        //{
        //    string? data = req.Headers.FirstOrDefault(x => x.Key == "x-ms-client-principal").Value.FirstOrDefault();

        //    if (string.IsNullOrWhiteSpace(data))
        //    {
        //        return null;
        //    }

        //    return ParseData(data);
        //}

        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        internal sealed record ClientPrincipal(string IdentityProvider, string UserId, string UserDetails, IEnumerable<string>? UserRoles);

        public static ClaimsPrincipal? ParseData(string data)
        {
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);

            ClientPrincipal? principal = JsonSerializer.Deserialize<ClientPrincipal>(json, _jsonSerializerOptions);

            if (principal is null)
            {
                return null;
            }

            var userRoles = principal.UserRoles?.Where(r => !string.Equals(r, "anonymous", StringComparison.CurrentCultureIgnoreCase)).Select(r => new Claim(ClaimTypes.Role, r));

            if (!principal.UserRoles?.Any() ?? true)
            {
                return new ClaimsPrincipal();
            }

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            if (userRoles is not null)
            {
                identity.AddClaims(userRoles);
            }

            return new ClaimsPrincipal(identity);
        }
    }
}