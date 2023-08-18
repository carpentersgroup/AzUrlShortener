using System.Security.Claims;

namespace Fizzibly.Auth
{
    public static class AuthValidator
    {
        private const string AAD_CLOUD_ISSUER_TEMPLATE = "https://login.microsoftonline.com/{0}/v2.0";
        private const string AAD_HYBRID_ISSUER_TEMPLATE = "https://sts.windows.net/{0}/";

        public static async Task<ClaimsPrincipal?> ValidateTokenAsync(
            string token,
            Microsoft.IdentityModel.Protocols.IConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration> configurationManager,
            IEnumerable<string>? tenantIds,
            Func<IEnumerable<string>, List<string>>? issuerTransformer = null,
            IEnumerable<string>? clientIds = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(tenantIds);

            var discoveryDocument = await configurationManager.GetConfigurationAsync(ct).ConfigureAwait(false);
            var signingKeys = discoveryDocument.SigningKeys;

            List<string> issuers;
            if (tenantIds is null || !tenantIds.Any())
            {
                issuers = new List<string> { AAD_CLOUD_ISSUER_TEMPLATE, AAD_HYBRID_ISSUER_TEMPLATE };
            }
            else
            {
                issuerTransformer ??= Transform;

                issuers = issuerTransformer(tenantIds);
            }

#if DEBUG
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
#endif

            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuers = issuers,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                // Allow for some drift in server time
                // (a lower value is better)
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            if (clientIds is not null)
            {
                validationParameters.ValidAudiences = clientIds;
                validationParameters.ValidateAudience = true;
            }

            try
            {
                return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out var rawValidatedToken);
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException)
            {
                // Logging, etc.

                return null;
            }
        }

        private static List<string> Transform(IEnumerable<string> tenantIds)
        {
            List<string> issuers = tenantIds.Select(tenantId => string.Format(AAD_CLOUD_ISSUER_TEMPLATE, tenantId)).ToList();
            issuers.AddRange(tenantIds.Select(tenantId => string.Format(AAD_HYBRID_ISSUER_TEMPLATE, tenantId)));
            return issuers;
        }
    }
}