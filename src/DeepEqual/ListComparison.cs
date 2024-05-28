namespace DeepEqual;

public class ListComparison : IComparison
{
    public IComparison Inner { get; }

    public ListComparison(IComparison inner)
    {
        Inner = inner;
    }

    public bool CanCompare(Type type1, Type type2)
    {
        if (!ReflectionCache.IsListType(type1) || !ReflectionCache.IsListType(type2))
            return false;

        return checkInnerCanCompare();

        bool checkInnerCanCompare()
        {
            var innerType1 = ReflectionCache.GetEnumerationType(type1);
            var innerType2 = ReflectionCache.GetEnumerationType(type2);

            return Inner.CanCompare(innerType1, innerType2);
        }
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? value1,
        object? value2
    )
    {
        if (value1 == null || value2 == null)
        {
            return (ComparisonResult.Inconclusive, context);
        }

        var list1 = ((IEnumerable)value1).Cast<object>().ToArray();
        var list2 = ((IEnumerable)value2).Cast<object>().ToArray();

        var length = list1.Length;

        if (length != list2.Length)
        {
            return (
                ComparisonResult.Fail,
                context.AddDifference(length, list2.Length, "Count", "Count")
            );
        }

        if (length == 0)
        {
            return (ComparisonResult.Pass, context);
        }

        return Enumerable
            .Range(0, length)
            .Select(i => (value1: list1[i], value2: list2[i], index: i))
            .Aggregate(
                (result: ComparisonResult.Inconclusive, context: context),
                (acc, x) =>
                {
                    var (newResult, newContext) = Inner.Compare(
                        context.VisitingIndex(x.index),
                        x.value1,
                        x.value2
                    );
                    return (
                        acc.result.Plus(newResult),
                        acc.context.MergeDifferencesFrom(newContext)
                    );
                }
            );
    }
}
