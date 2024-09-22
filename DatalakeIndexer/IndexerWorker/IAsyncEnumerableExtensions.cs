public static class IAsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        await foreach (var item in source)
        {
            yield return selector(item);
        }
    }

    public static async IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> source, long count)
    {
        await foreach (var item in source)
        {
            if (--count < 0)
            {
                yield break;
            }

            yield return item;
        }
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }
}