namespace Lab1.ZipMethods;

public static class AsyncEnumerableExtension
{
    public static async IAsyncEnumerable<TResult> AsyncZipMore<TSource, TResult>(
        this IAsyncEnumerable<TSource> first,
        Func<TSource[], TResult> resultSelector,
        params IEnumerable<TSource>[] others)
    {
        var enumerators = new List<IAsyncEnumerator<TSource>> { first.GetAsyncEnumerator() };
        enumerators.AddRange(others.Select(other => other.ToAsyncEnumerable().GetAsyncEnumerator()));

        try
        {
            while ((await Task.WhenAll(enumerators.Select(e => e.MoveNextAsync().AsTask())).ConfigureAwait(false)).All(result => result))
            {
                yield return resultSelector(enumerators.Select(e => e.Current).ToArray());
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<TSource> enumerator in enumerators)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (T item in source)
        {
            yield return item;
            await Task.Yield();
        }
    }
}