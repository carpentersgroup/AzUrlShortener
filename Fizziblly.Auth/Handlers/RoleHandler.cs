using Fizzibly.Auth.Handlers.Abstractions;
using Fizzibly.Auth.Models;
using Fizzibly.Auth.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Fizzibly.Auth.Handlers
{
    public class RoleHandler : BaseHandler, IHandler
    {
        private readonly IEnumerable<string> _requiredRoles;
        private readonly ILogger<RoleHandler> _logger;

        public RoleHandler(IOptions<JwtSettings> jwtSettings, ILogger<RoleHandler> logger)
        {
            _requiredRoles = jwtSettings.Value.RequiredRoles;
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

                AuthResult authResult = RoleValidator.CheckAuthRole(principal.Principal, _requiredRoles, _logger);

                if (_next is null || authResult != AuthResult.Ok)
                {
                    return authResult;
                }

                return await _next.Handle(principal).ConfigureAwait(false);
            }
            else
            {
                if (_next is null)
                {
                    return AuthResult.Ok;
                }

                return await _next.Handle(request).ConfigureAwait(false);
            }
        }
    }
}