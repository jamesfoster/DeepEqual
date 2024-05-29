namespace DeepEqual;

public class SetComparison : IComparison
{
    public IComparison Inner { get; set; }

    public SetComparison(IComparison inner)
    {
        Inner = inner;
    }

    public bool CanCompare(Type leftType, Type rightType)
    {
        var isSetType1 = ReflectionCache.IsSetType(leftType);
        var isSetType2 = ReflectionCache.IsSetType(rightType);

        if (!isSetType1 && !isSetType2)
            return false;

        if (!isSetType1 && !ReflectionCache.IsListType(leftType))
            return false;

        if (!isSetType2 && !ReflectionCache.IsListType(rightType))
            return false;

        var leftElementType = ReflectionCache.GetEnumerationType(leftType);
        var rightElementType = ReflectionCache.GetEnumerationType(rightType);

        return Inner.CanCompare(leftElementType, rightElementType);
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

        var leftSet = ((IEnumerable)leftValue).Cast<object>().ToArray();
        var rightSet = ((IEnumerable)rightValue).Cast<object>().ToArray();

        if (leftSet.Length != rightSet.Length)
        {
            return (
                ComparisonResult.Fail,
                context.AddDifference(leftSet.Length, rightSet.Length, "Count", "Count")
            );
        }

        if (leftSet.Length == 0)
        {
            return (ComparisonResult.Pass, context);
        }

        return SetsEqual(context, leftSet, rightSet);
    }

    private (ComparisonResult result, IComparisonContext context) SetsEqual(
        IComparisonContext context,
        object[] leftSet,
        object[] rightSet
    )
    {
        var expected = rightSet.ToList();
        var extra = new List<object>();

        foreach (var obj in leftSet)
        {
            var innerContext = new ComparisonContext();
            var found = expected.FirstOrDefault(e =>
                Inner.Compare(innerContext, obj, e).result == ComparisonResult.Pass
            );

            if (found != null)
                expected.RemoveAll(x => ReferenceEquals(x, found));
            else
                extra.Add(obj);
        }

        var equal = expected.Count == 0 && extra.Count == 0;

        if (!equal)
        {
            return (
                ComparisonResult.Fail,
                context.AddDifference(new SetDifference(context.Breadcrumb, expected, extra))
            );
        }

        return (ComparisonResult.Pass, context);
    }
}
