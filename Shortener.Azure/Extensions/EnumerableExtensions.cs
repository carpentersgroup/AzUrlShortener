namespace Shortener.AzureServices.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
        this IEnumerable<TSource> source,
        int batchSize)
        {
            var batch = new List<TSource>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<TSource>(batchSize);
                }
            }

            if (batch.Any()) yield return batch;
        }
    }
}
