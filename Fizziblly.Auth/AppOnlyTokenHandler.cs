using System.Security.Claims;

namespace Fizzibly.Auth
{
    public class AppOnlyTokenHandler : BaseHandler, IHandler
    {
        public Task<AuthResult> Handle(object request)
        {
            if (request is JwtAuthRequest principal)
            {
                if (principal.Principal == null)
                {
                    return Task.FromResult(AuthResult.Unauthorized);
                }

                if (IsAppOnlyToken(principal.Principal))
                {
                    if (this._next is null)
                    {
                        return Task.FromResult(AuthResult.Ok);
                    }

                    return this._next.Handle(new AppOnlyAuthRequest(principal.HttpRequestMessage, principal.Principal, principal.Token));
                }
                else
                {
                    if (this._next is null)
                    {
                        return Task.FromResult(AuthResult.Ok);
                    }

                    return this._next.Handle(new UserAuthRequest(principal.HttpRequestMessage, principal.Principal, principal.Token));
                }
            }
            else
            {
                if (this._next is null)
                {
                    return Task.FromResult(AuthResult.Ok);
                }

                return this._next.Handle(request);
            }
        }

        private static bool IsAppOnlyToken(ClaimsPrincipal principal)
        {
            string? oid = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (string.IsNullOrEmpty(oid))
            {
                oid = principal.FindFirst("oid")?.Value;
            }

            string? sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                sub = principal.FindFirst("sub")?.Value;
            }

            bool isAppOnlyToken = oid == sub;
            return isAppOnlyToken;
        }
    }
}