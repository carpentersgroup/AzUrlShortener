using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Fizzibly.Auth
{
    public class RoleHandler : BaseHandler, IHandler
    {
        private readonly IEnumerable<string> _requiredRoles;
        private readonly ILogger<RoleHandler> _logger;

        public RoleHandler(IOptions<JwtSettings> jwtSettings, ILogger<RoleHandler> logger)
        {
            this._requiredRoles = jwtSettings.Value.RequiredRoles;
            _logger = logger;
        }

        public async Task<AuthResult> Handle(object request)
        {
            if (request is AppOnlyAuthRequest principal)
            {
                if (principal.Principal == null)
                {
                    return AuthResult.Unauthorized;
                }

                AuthResult authResult = CheckAuthRole(principal.Principal, _requiredRoles);

                if (this._next is null || authResult != AuthResult.Ok)
                {
                    return authResult;
                }

                return await _next.Handle(principal).ConfigureAwait(false);
            }
            else
            {
                if (this._next is null)
                {
                    return AuthResult.Ok;
                }

                return await _next.Handle(request).ConfigureAwait(false);
            }
        }

        private AuthResult CheckAuthRole(ClaimsPrincipal principal, IEnumerable<string> requiredRoles)
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