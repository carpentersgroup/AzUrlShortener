using Fizzibly.Auth.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Fizzibly.Auth.Validators
{
    public static class RoleValidator
    {

        internal static AuthResult CheckAuthRole(ClaimsPrincipal principal, IEnumerable<string> requiredRoles, ILogger _logger)
        {
            if (principal == null)
            {
                _logger.LogWarning("No principal.");
                return AuthResult.Unauthorized;
            }

            if (principal.Identity == null)
            {
                _logger.LogWarning("No identity.");
                return AuthResult.Unauthorized;
            }

            if (!principal.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Request was not authenticated.");
                return AuthResult.Unauthorized;
            }

            const string ROLES_CLAIM = "roles";

            var allRoles = principal.Claims.Where(
                    c => c.Type == ROLES_CLAIM || c.Type == ClaimTypes.Role)
                    .SelectMany(c => c.Value.Split(' '));

            if (!allRoles.Any())
            {
                _logger.LogError("Role not found");

                return AuthResult.Forbidden;
            }

            foreach (string requiredRole in requiredRoles)
            {
                if (!allRoles.Contains(requiredRole))
                {
                    _logger.LogError("Required role missing");
                    return AuthResult.Forbidden;
                }
            }

            _logger.LogInformation("Authenticated role.");

            return AuthResult.Ok;
        }
    }
}