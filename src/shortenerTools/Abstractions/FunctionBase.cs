using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace shortenerTools.Abstractions
{
    public abstract class FunctionBase
    {
        public virtual async Task<(bool isValidRequest, IActionResult invalidResult, T requestType)> ValidateRequestAsync<T>(
            HttpRequestMessage req, ClaimsPrincipal principal, ILogger log) where T : class, new()
        {
            var invalidRequest = Utility.CheckUserImpersonatedAuth(principal, log);

            if (invalidRequest != null)
            {
                return (false, invalidRequest, null as T);
            }

            if (req == null)
            {
                return (false, new NotFoundResult(), null);
            }

            var result = await req.Content.ReadAsAsync<T>();
            if (result == null)
            {
                return (false, new NotFoundResult(), null);
            }

            return (true, null, result);
        }

        public virtual async Task<T> ParseRequestAsync<T>(HttpRequestMessage req) where T : class, new()
        {
            if (req == null)
            {
                return null;
            }

            return await req.Content.ReadAsAsync<T>().ConfigureAwait(false);
        }

        public virtual IActionResult ValidateAuth(ClaimsPrincipal principal, ILogger log)
        {
            return Utility.CheckUserImpersonatedAuth(principal, log);
        }
    }
}