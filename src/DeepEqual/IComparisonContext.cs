namespace DeepEqual;

public interface IComparisonContext
{
    ImmutableList<Difference> Differences { get; }
    BreadcrumbPair Breadcrumb { get; }

    IComparisonContext AddDifference(Difference difference);
    IComparisonContext SetBreadcrumb(BreadcrumbPair breadcrumb);
}
