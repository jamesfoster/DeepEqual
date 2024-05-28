namespace DeepEqual;

public static class ComparisonContextExtensions
{
    public static IComparisonContext AddDifference(
        this IComparisonContext context,
        object? value1,
        object? value2
    )
    {
        return AddDifference(context, value1, value2, null);
    }

    public static IComparisonContext AddDifference(
        this IComparisonContext context,
        object? value1,
        object? value2,
        string? childProperty
    )
    {
        return context.AddDifference(
            new BasicDifference(context.Breadcrumb, value1, value2, childProperty)
        );
    }

    public static IComparisonContext VisitingProperty(
        this IComparisonContext context,
        string propertyName
    )
    {
        return context.SetBreadcrumb(context.Breadcrumb.Dot(propertyName));
    }

    public static IComparisonContext VisitingIndex(this IComparisonContext context, object index)
    {
        return context.SetBreadcrumb(context.Breadcrumb.Index($"{index}"));
    }

    public static IComparisonContext MergeDifferencesFrom(
        this IComparisonContext context,
        IComparisonContext child
    )
    {
        return child.Differences.Aggregate(context, (c, d) => c.AddDifference(d));
    }
}
