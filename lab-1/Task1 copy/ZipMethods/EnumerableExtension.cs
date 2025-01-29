namespace Lab1.ZipMethods;

public static class EnumerableExtension
{
    public static IEnumerable<TResult> ZipMore<TSource, TResult>(
        this IEnumerable<TSource> first,
        Func<TSource[], TResult> resultSelector,
        params IEnumerable<TSource>[] others)
    {
        var enumerators = new List<IEnumerator<TSource>> { first.GetEnumerator() };
        enumerators.AddRange(others.Select(e => e.GetEnumerator()));

        try
        {
            while (enumerators.All(e => e.MoveNext()))
                yield return resultSelector(enumerators.Select(e => e.Current).ToArray());
        }
        finally
        {
            foreach (IEnumerator<TSource> enumerator in enumerators)
                enumerator.Dispose();
        }
    }
}
