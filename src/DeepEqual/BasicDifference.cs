namespace DeepEqual;

public record BasicDifference(
    BreadcrumbPair Breadcrumb,
    object? Value1,
    object? Value2,
    string? LeftChildProperty,
    string? RightChildProperty
) : Difference(Breadcrumb);
