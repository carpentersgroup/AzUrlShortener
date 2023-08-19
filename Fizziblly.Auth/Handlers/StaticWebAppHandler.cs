using System.Data;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Fizzibly.Auth.Handlers.Abstractions;
using Fizzibly.Auth.Models;
using Fizzibly.Auth.Validators;

namespace Fizzibly.Auth.Handlers
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

                if (_next is null)
                {
                    return AuthResult.Ok;
                }

                return await _next.Handle(request).ConfigureAwait(false);
            }

            if (_next is null)
            {
                return AuthResult.Ok;
            }

            return await _next.Handle(request).ConfigureAwait(false);
        }

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

            return MsClientPrincipalParser.ParseData(data);
        }
    }
}