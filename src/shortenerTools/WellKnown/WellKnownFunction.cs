using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System.Threading.Tasks;

namespace shortenerTools.WellKnown
{
    internal class WellKnownFunction
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public WellKnownFunction(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("WellKnown")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WellKnown/{filename}")] HttpRequest req, string filename,
        ILogger log)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var content = await _storageTableHelper.GetWellKnownContent(filename);

                string contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(filename, out contentType);
                contentType = contentType ?? "application/octet-stream";

                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(content), contentType);
                //return new OkObjectResult(content);
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
