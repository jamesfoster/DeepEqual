namespace DeepEqual;

public class ComplexObjectComparison : IComparison
{
    internal bool IgnoreUnmatchedProperties { get; }
    internal IReadOnlyList<Func<PropertyPair, bool>> IgnoredProperties { get; }
    internal IReadOnlyList<Func<Type, Type, string, string?>> MappedProperties { get; }

    public ComplexObjectComparison(
        bool ignoreUnmatchedProperties,
        List<Func<PropertyPair, bool>> ignoredProperties,
        List<Func<Type, Type, string, string?>> mappedProperties
    )
    {
        IgnoreUnmatchedProperties = ignoreUnmatchedProperties;
        IgnoredProperties = ignoredProperties;
        MappedProperties = mappedProperties;
    }

    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        return (leftType.IsClass && rightType.IsClass)
            || ReflectionCache.IsValueTypeWithReferenceFields(leftType)
            || ReflectionCache.IsValueTypeWithReferenceFields(rightType);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        var comparer = new ComplexObjectComparer(
            IgnoreUnmatchedProperties,
            IgnoredProperties,
            MappedProperties
        );

        return comparer.CompareObjects(context, leftValue, rightValue);
    }
}
