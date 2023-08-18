namespace Fizzibly.Auth
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

                if (this._next is null)
                {
                    return AuthResult.Ok;
                }

                return await this._next.Handle(principal with { Token = token }).ConfigureAwait(false);
            }

            if (this._next is null)
            {
                return AuthResult.Ok;
            }

            return await this._next.Handle(request).ConfigureAwait(false);
        }
    }
}