namespace DeepEqual;

public record SetDifference(
    BreadcrumbPair Breadcrumb,
    ImmutableList<object> Expected,
    ImmutableList<object> Extra
) : Difference(Breadcrumb)
{
    public SetDifference(
        BreadcrumbPair breadcrumb,
        IEnumerable<object> expected,
        IEnumerable<object> extra
    )
        : this(breadcrumb, expected.ToImmutableList(), extra.ToImmutableList()) { }
}
