using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Fizzibly.Auth
{
    public class UserHandler : BaseHandler, IHandler
    {
        private readonly ILogger<UserHandler> _logger;

        public UserHandler(ILogger<UserHandler> logger)
        {
            _logger = logger;
        }

        public async Task<AuthResult> Handle(object request)
        {
            if (request is UserAuthRequest principal)
            {
                if (principal.Principal == null)
                {
                    return AuthResult.Unauthorized;
                }

                AuthResult authResult = CheckUserImpersonatedAuth(principal.Principal);

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

                return await this._next.Handle(request).ConfigureAwait(false);
            }
        }

        private AuthResult CheckUserImpersonatedAuth(ClaimsPrincipal principal)
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

            var nameClaim = principal.FindFirst(ClaimTypes.GivenName);
            if (nameClaim is null)
            {
                _logger.LogError("Claim not Found");
                return AuthResult.Forbidden;
            }

            _logger.LogInformation("Authenticated user {user}.", nameClaim.Value);
            return AuthResult.Ok;
        }
    }
}