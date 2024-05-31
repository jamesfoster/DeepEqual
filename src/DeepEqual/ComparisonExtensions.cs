namespace DeepEqual;

public static class ComparisonExtensions
{
    public static T OfType<T>(this IComparison source)
        where T : class, IComparison
    {
        return source.OfTypeInternal<T>()
            ?? throw new ComparisonNotFoundException(
                "Expected to find a comparison of type " + typeof(T).Name
            );
    }

    private static T? OfTypeInternal<T>(this IComparison source)
        where T : class, IComparison
    {
        if (source is T result)
            return result;

        if (source is CycleGuard guard)
            return guard.Inner.OfTypeInternal<T>();

        if (source is CompositeComparison composite)
        {
            return composite
                .Comparisons.Select(c => c.OfTypeInternal<T>())
                .FirstOrDefault(c => c != null);
        }

        return null;
    }
}
