using System.Security.Claims;

namespace Fizzibly.Auth.Validators
{
    public static class AppOnlyValidator
    {
        internal static bool IsAppOnlyToken(ClaimsPrincipal principal)
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