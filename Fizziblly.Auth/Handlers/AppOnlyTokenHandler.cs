using System.Security.Claims;
using Fizzibly.Auth.Handlers.Abstractions;
using Fizzibly.Auth.Models;
using Fizzibly.Auth.Validators;

namespace Fizzibly.Auth.Handlers
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

                if (AppOnlyValidator.IsAppOnlyToken(principal.Principal))
                {
                    if (_next is null)
                    {
                        return Task.FromResult(AuthResult.Ok);
                    }

                    return _next.Handle(new AppOnlyAuthRequest(principal.HttpRequestMessage, principal.Principal, principal.Token));
                }
                else
                {
                    if (_next is null)
                    {
                        return Task.FromResult(AuthResult.Ok);
                    }

                    return _next.Handle(new UserAuthRequest(principal.HttpRequestMessage, principal.Principal, principal.Token));
                }
            }
            else
            {
                if (_next is null)
                {
                    return Task.FromResult(AuthResult.Ok);
                }

                return _next.Handle(request);
            }
        }
    }
}