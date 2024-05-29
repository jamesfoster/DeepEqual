namespace DeepEqual;

public class DictionaryComparison : IComparison
{
    public IComparison KeyComparer { get; set; }
    public IComparison ValueComparer { get; set; }

    public DictionaryComparison(IComparison keyComparer, IComparison valueComparer)
    {
        KeyComparer = keyComparer;
        ValueComparer = valueComparer;
    }

    public bool CanCompare(Type leftType, Type rightType)
    {
        return ReflectionCache.IsDictionaryType(leftType)
            && ReflectionCache.IsDictionaryType(rightType);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        if (leftValue is not IDictionary leftDict)
        {
            return (ComparisonResult.Inconclusive, context);
        }
        if (rightValue is not IDictionary rightDict)
        {
            return (ComparisonResult.Inconclusive, context);
        }

        if (leftDict.Count != rightDict.Count)
        {
            return (
                ComparisonResult.Fail,
                context.AddDifference(leftDict.Count, rightDict.Count, "Count", "Count")
            );
        }

        var rightDictEntries = CastToDictionaryEntries(rightDict)
            .ToDictionary(e => e.Key, e => (object?)e.Value);

        if (leftDict.Count == 0)
        {
            return (ComparisonResult.Pass, context);
        }

        var newContext = context;
        var results = new List<ComparisonResult>();

        foreach (DictionaryEntry leftEntry in leftDict)
        {
            var key = FindKey(rightDictEntries, leftEntry.Key);

            if (key == null)
            {
                var difference = new MissingEntryDifference(
                    context.Breadcrumb,
                    MissingSide.Right,
                    leftEntry.Key,
                    leftEntry.Value
                );

                context.AddDifference(difference);

                continue;
            }

            var value = rightDictEntries[key];
            rightDictEntries.Remove(key);

            var (result, innerContext) = ValueComparer.Compare(
                context.VisitingIndex(key),
                leftEntry.Value,
                value
            );

            results.Add(result);
            newContext = newContext.MergeDifferencesFrom(innerContext);
        }

        if (rightDictEntries.Count == 0)
            return (results.ToResult(), newContext);

        foreach (var entry in rightDictEntries)
        {
            var difference = new MissingEntryDifference(
                context.Breadcrumb,
                MissingSide.Left,
                entry.Key,
                entry.Value
            );

            newContext = newContext.AddDifference(difference);
        }

        return (ComparisonResult.Fail, newContext);
    }

    private static IEnumerable<DictionaryEntry> CastToDictionaryEntries(IDictionary source)
    {
        foreach (DictionaryEntry entry in source)
            yield return entry;
    }

    private object? FindKey(IDictionary<object, object?> dictionary, object key)
    {
        var tempContext = new ComparisonContext();

        foreach (var key2 in dictionary.Keys)
        {
            var (result, _) = KeyComparer.Compare(tempContext, key, key2);
            if (result == ComparisonResult.Pass)
                return key2;
        }

        return null;
    }
}
