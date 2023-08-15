namespace Shortener.AzureServices
{
    public static class IAsyncEnumerableExtensions
    {
        public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var enumerator = enumerable.GetAsyncEnumerator();
            try
            {                
                if(!await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    return default;
                }

                return enumerator.Current;
            }
            finally
            {
                if (enumerator != null)
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
