namespace DeepEqual;

public interface IComparisonContext
{
    ImmutableList<Difference> Differences { get; }
    string Breadcrumb { get; }

    IComparisonContext AddDifference(Difference difference);
    IComparisonContext SetBreadcrumb(string breadcrumb);
}
