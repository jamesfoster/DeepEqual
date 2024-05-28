namespace DeepEqual;

public class ComparisonContext : IComparisonContext
{
    public ImmutableList<Difference> Differences { get; }
    public BreadcrumbPair Breadcrumb { get; }

    public ComparisonContext()
        : this(BreadcrumbPair.Empty) { }

    public ComparisonContext(BreadcrumbPair breadcrumb)
        : this(null, breadcrumb) { }

    public ComparisonContext(ImmutableList<Difference>? differences, BreadcrumbPair breadcrumb)
    {
        Differences = differences ?? ImmutableList<Difference>.Empty;
        Breadcrumb = breadcrumb;
    }

    public IComparisonContext AddDifference(Difference difference)
    {
        var newDifferences = Differences.Add(difference);

        return new ComparisonContext(newDifferences, Breadcrumb);
    }

    public IComparisonContext SetBreadcrumb(BreadcrumbPair breadcrumb)
    {
        return new ComparisonContext(Differences, breadcrumb);
    }
}
