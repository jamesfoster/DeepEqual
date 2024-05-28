namespace DeepEqual;

public record BasicDifference(
    string Breadcrumb,
    object? Value1,
    object? Value2,
    string? ChildProperty
) : Difference(Breadcrumb);
