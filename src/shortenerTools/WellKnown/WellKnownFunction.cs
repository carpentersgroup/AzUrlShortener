using Cloud5mins.domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System.IO;
using System.Threading.Tasks;

namespace shortenerTools.WellKnown
{
    internal class WellKnownFunction
    {
        private readonly IConfiguration _configuration;
        private readonly IStorageTableHelper _storageTableHelper;

        public WellKnownFunction(IConfiguration configuration, IStorageTableHelper storageTableHelper)
        {
            _configuration = configuration;
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("WellKnown")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, string filename,
        ILogger log)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var content = await _storageTableHelper.GetWellKnownContent(filename);

                return new OkObjectResult(content);
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
