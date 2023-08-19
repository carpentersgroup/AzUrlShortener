using Fizzibly.Auth.Handlers.Abstractions;
using Fizzibly.Auth.Models;

namespace Fizzibly.Auth.Handlers
{
    public class TokenHandler : BaseHandler, IHandler
    {
        public async Task<AuthResult> Handle(object request)
        {
            if (request is JwtAuthRequest principal)
            {
                var token = principal.HttpRequestMessage.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.FirstOrDefault()?.Split(" ")[^1];
                if (string.IsNullOrWhiteSpace(token))
                {
                    return AuthResult.Unauthorized;
                }

                if (_next is null)
                {
                    return AuthResult.Ok;
                }

                return await _next.Handle(principal with { Token = token }).ConfigureAwait(false);
            }

            if (_next is null)
            {
                return AuthResult.Ok;
            }

            return await _next.Handle(request).ConfigureAwait(false);
        }
    }
}