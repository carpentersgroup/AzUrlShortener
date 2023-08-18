using Microsoft.Extensions.Options;

namespace Fizzibly.Auth
{
    public class JwtHandler : BaseHandler, IHandler
    {
        private readonly JwtSettings _jwtSettings;

        public JwtHandler(IOptions<JwtSettings> jwtSettings)
        {
            this._jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResult> Handle(object request)
        {
            if (request is JwtAuthRequest principal)
            {
                if (string.IsNullOrEmpty(principal.Token))
                {
                    return AuthResult.Unauthorized;
                }

                if(this._next is null)
                {
                    return AuthResult.Ok;
                }

                var configurationManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                               _jwtSettings.MetadataAddress, new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever());

                var validatedClaimsPrincipal = await AuthValidator.ValidateTokenAsync(principal.Token, configurationManager, _jwtSettings.AllowedTenants, null, _jwtSettings.AllowedClients, CancellationToken.None).ConfigureAwait(false);

                if (validatedClaimsPrincipal is null)
                {
                    return AuthResult.Unauthorized;
                }

                return await this._next.Handle(principal with { Principal = validatedClaimsPrincipal }).ConfigureAwait(false);
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
    }
}