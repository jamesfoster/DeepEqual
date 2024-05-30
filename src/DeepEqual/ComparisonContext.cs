namespace DeepEqual;

public class ComparisonContext : IComparisonContext
{
    public ImmutableList<Difference> Differences { get; }
    public BreadcrumbPair Breadcrumb { get; }

    public ComparisonContext()
        : this(new BreadcrumbPair("Left", "Right")) { }

    public ComparisonContext(BreadcrumbPair breadcrumb)
        : this(ImmutableList<Difference>.Empty, breadcrumb) { }

    public ComparisonContext(ImmutableList<Difference> differences, BreadcrumbPair breadcrumb)
    {
        Differences = differences;
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
