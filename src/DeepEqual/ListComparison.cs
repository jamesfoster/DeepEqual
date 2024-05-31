namespace DeepEqual;

public class ListComparison : IComparison
{
    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        if (!ReflectionCache.IsListType(leftType) || !ReflectionCache.IsListType(rightType))
            return false;

        return checkInnerCanCompare();

        bool checkInnerCanCompare()
        {
            var innerLeftType = ReflectionCache.GetEnumerationType(leftType);
            var innerRightType = ReflectionCache.GetEnumerationType(rightType);

            return context.CanCompare(innerLeftType, innerRightType);
        }
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        if (leftValue == null || rightValue == null)
        {
            return (ComparisonResult.Inconclusive, context);
        }

        var list1 = ((IEnumerable)leftValue).Cast<object>().ToArray();
        var list2 = ((IEnumerable)rightValue).Cast<object>().ToArray();

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
            .Select(i => (leftValue: list1[i], rightValue: list2[i], index: i))
            .Aggregate(
                (result: ComparisonResult.Inconclusive, context: context),
                (acc, x) =>
                {
                    var (newResult, newContext) = context
                        .VisitingIndex(x.index)
                        .Compare(x.leftValue, x.rightValue);
                    return (
                        acc.result.Plus(newResult),
                        acc.context.MergeDifferencesFrom(newContext)
                    );
                }
            );
    }
}
