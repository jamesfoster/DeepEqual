namespace DeepEqual;

public record MissingEntryDifference(
    BreadcrumbPair Breadcrumb,
    MissingSide Side,
    object Key,
    object? Value
) : Difference(Breadcrumb);

public enum MissingSide
{
    Left,
    Right
}
