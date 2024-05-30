namespace DeepEqual;

public record SetDifference(
    BreadcrumbPair Breadcrumb,
    ImmutableList<object> MissingInLeft,
    ImmutableList<object> MissingInRight
) : Difference(Breadcrumb)
{
    public SetDifference(
        BreadcrumbPair breadcrumb,
        IEnumerable<object> missingInLeft,
        IEnumerable<object> missingInRight
    )
        : this(breadcrumb, missingInLeft.ToImmutableList(), missingInRight.ToImmutableList()) { }
}
