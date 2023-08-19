namespace Fizzibly.Auth.Worker
{
    internal sealed class AppOnlyTokenMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestData = await context.GetHttpRequestDataAsync();

            string correlationId;
            if (requestData!.Headers.TryGetValues("x-correlationId", out var values))
            {
                correlationId = values.First();
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
            }

            await next(context);

            context.GetHttpResponseData()?.Headers.Add("x-correlationId", correlationId);
        }
    }
}