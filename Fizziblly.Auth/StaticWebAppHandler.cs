using System.Data;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace Fizzibly.Auth
{
    public class StaticWebAppHandler : BaseHandler, IHandler
    {
        public async Task<AuthResult> Handle(object request)
        {
            if (request is JwtAuthRequest authRequest)
            {
                ClaimsPrincipal principal = authRequest.Principal!;

                ClaimsPrincipal? clientPrincipal = Parse(authRequest.HttpRequestMessage);

                IEnumerable<Claim>? newClaims = clientPrincipal?.Claims.Except(principal.Claims);

                if (newClaims is null || newClaims.Any())
                {
                    return AuthResult.Forbidden;
                }

                if (principal.Identity is ClaimsIdentity claimsIdentity)
                {
                    claimsIdentity.AddClaims(newClaims);
                }

                if (this._next is null)
                {
                    return AuthResult.Ok;
                }

                return await this._next.Handle(request).ConfigureAwait(false);
            }

            if (this._next is null)
            {
                return AuthResult.Ok;
            }

            return await this._next.Handle(request).ConfigureAwait(false);
        }

        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private sealed record ClientPrincipal(string IdentityProvider, string UserId, string UserDetails, IEnumerable<string>? UserRoles);

        //public static ClaimsPrincipal? Parse(Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        //{
        //    string? data = req.Headers.FirstOrDefault(x => x.Key == "x-ms-client-principal").Value.FirstOrDefault();

        //    if (string.IsNullOrWhiteSpace(data))
        //    {
        //        return null;
        //    }

        //    return ParseData(data);
        //}

        public static ClaimsPrincipal? Parse(Microsoft.AspNetCore.Http.HttpRequest req)
        {
            if (!req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                return null;
            }

            var data = header[0];

            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            return ParseData(data);
        }

        private static ClaimsPrincipal? ParseData(string data)
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