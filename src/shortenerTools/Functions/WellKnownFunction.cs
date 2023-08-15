using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Shortener.AzureServices;
using System.Threading.Tasks;

namespace ShortenerTools.Functions
{
    internal class WellKnownFunction
    {
        private readonly IStorageTableHelper _storageTableHelper;
        private static FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        public WellKnownFunction(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("WellKnown")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WellKnown/{filename}")] Microsoft.AspNetCore.Http.HttpRequest req, string filename,
        ILogger log)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var content = await _storageTableHelper.GetWellKnownContentAsync(filename).ConfigureAwait(false);

                fileExtensionContentTypeProvider.TryGetContentType(filename, out string? contentType);
                contentType = contentType ?? "application/octet-stream";

                return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(content), contentType);
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }

}
