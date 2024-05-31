namespace DeepEqual;

public class ComparisonContext : IComparisonContext
{
    public ImmutableList<Difference> Differences { get; }
    public BreadcrumbPair Breadcrumb { get; }

    public IComparison RootComparison { get; }

    public ComparisonContext(IComparison rootComparison)
        : this(rootComparison, new BreadcrumbPair("Left", "Right")) { }

    public ComparisonContext(IComparison rootComparison, BreadcrumbPair breadcrumb)
        : this(rootComparison, ImmutableList<Difference>.Empty, breadcrumb) { }

    public ComparisonContext(
        IComparison rootComparison,
        ImmutableList<Difference> differences,
        BreadcrumbPair breadcrumb
    )
    {
        RootComparison = rootComparison;
        Differences = differences;
        Breadcrumb = breadcrumb;
    }

    public IComparisonContext AddDifference(Difference difference)
    {
        var newDifferences = Differences.Add(difference);

        return new ComparisonContext(RootComparison, newDifferences, Breadcrumb);
    }

    public IComparisonContext SetBreadcrumb(BreadcrumbPair breadcrumb)
    {
        return new ComparisonContext(RootComparison, Differences, breadcrumb);
    }

    public IComparisonContext NewEmptyContext()
    {
        return new ComparisonContext(RootComparison);
    }

    public bool CanCompare(Type left, Type right)
    {
        return RootComparison.CanCompare(this, left, right);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        object? left,
        object? right
    )
    {
        return RootComparison.Compare(this, left, right);
    }
}
