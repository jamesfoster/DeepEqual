namespace DeepEqual;

public record BasicDifference(
    BreadcrumbPair Breadcrumb,
    object? LeftValue,
    object? RightValue,
    string? LeftChildProperty,
    string? RightChildProperty
) : Difference(Breadcrumb);
