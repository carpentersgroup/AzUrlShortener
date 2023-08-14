using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Shortener.Core;
using Shortener.Core.Redirect;
using ShortenerTools.Extensions;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    public class UrlRedirect
    {
        private readonly IUrlRedirectService _urlRedirectService;
        private readonly IUserIpLocationService _userIpLocationService;

        public UrlRedirect(IUrlRedirectService urlRedirectService, IUserIpLocationService userIpLocationService)
        {
            _urlRedirectService = urlRedirectService;
            _userIpLocationService = userIpLocationService;
        }

        [FunctionName("UrlRedirect")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlRedirect/{shortUrl}")] Microsoft.AspNetCore.Http.HttpRequest req,
            string shortUrl,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");
            var ipAddress = this._userIpLocationService.GetClientIpn(req);
            Result<string, RedirectStatus> result = await _urlRedirectService.ProcessAsync(ipAddress, req.GetHostFromRequest(), shortUrl).ConfigureAwait(false);
            return result switch
            {
                { Status: RedirectStatus.Success } => new RedirectResult(result.Value!, false),
                { Status: RedirectStatus.NotFound } => new RedirectResult(result.Value!, false),
                { Status: RedirectStatus.Invalid } => new RedirectResult(result.Value!, false),
                _ => new NotFoundResult()
            };
        }
    }
}
