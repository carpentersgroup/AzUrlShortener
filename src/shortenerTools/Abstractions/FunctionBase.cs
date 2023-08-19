using Fizzibly.Auth;
using Fizzibly.Auth.Handlers;
using Fizzibly.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shortener.Core.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShortenerTools.Abstractions
{
    public abstract class FunctionBase
    {
        protected readonly UrlShortenerConfiguration _configuration;
        private readonly HandlerContainer _handlerContainer;

        protected FunctionBase(
            IOptions<UrlShortenerConfiguration> options,
            HandlerContainer authHandlerContainer)
        {
            _configuration = options.Value;
            _handlerContainer = authHandlerContainer;
        }

        public virtual async Task<T?> ParseRequestAsync<T>(HttpRequestMessage req) where T : class, new()
        {
            if (req == null)
            {
                return null;
            }

            return await req.Content.ReadAsAsync<T>().ConfigureAwait(false);
        }

        public static string GetAuthorityFromRequest(Microsoft.AspNetCore.Http.HttpRequest req)
        {
            return req.Host.Value;
        }

        public static string GetBaseUrlFromUri(System.Uri uri)
        {
            return uri.Scheme + "://" + uri.Authority;
        }

        public static string GetUrlFromRequest(Microsoft.AspNetCore.Http.HttpRequest req)
        {
            return req.Scheme + "://" + req.Host.Value;
        }

        public static string GetUrlFromRequest(Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        {
            return req.Url.Scheme + "://" + req.Url.Host;
        }

        public async Task<IActionResult?> HandleAuth(ClaimsPrincipal principal, Microsoft.AspNetCore.Http.HttpRequest requestMessage)
        {
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                return null;
            }   
#endif
            AuthResult authResult = await _handlerContainer.Handler.Handle(new JwtAuthRequest(requestMessage, principal, null)).ConfigureAwait(false);

            return authResult switch
            {
                AuthResult.Ok => null,
                AuthResult.Unauthorized => new UnauthorizedResult(),
                AuthResult.Forbidden => new ForbidResult(),
                AuthResult.BadRequest => new BadRequestResult(),
                _ => throw new Exceptions.SurprisingAuthResultException(authResult)
            };
        }
    }
}