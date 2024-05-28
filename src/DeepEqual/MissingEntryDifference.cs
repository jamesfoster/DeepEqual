namespace DeepEqual;

public record MissingEntryDifference(string Breadcrumb, MissingSide Side, object Key, object? Value)
    : Difference(Breadcrumb);

public enum MissingSide
{
    Actual,
    Expected
}
