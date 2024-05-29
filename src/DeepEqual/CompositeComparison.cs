﻿namespace DeepEqual;

public class CompositeComparison : IComparison
{
    public List<IComparison> Comparisons { get; set; }

    public CompositeComparison()
        : this(Enumerable.Empty<IComparison>()) { }

    public CompositeComparison(IEnumerable<IComparison> comparers)
    {
        Comparisons = comparers.ToList();
    }

    public void Add(IComparison comparison)
    {
        Comparisons.Add(comparison);
    }

    public void AddRange(params IComparison[] comparisons)
    {
        Comparisons.AddRange(comparisons);
    }

    public bool CanCompare(Type leftType, Type rightType)
    {
        return true;
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        if (leftValue == null && rightValue == null)
        {
            return (ComparisonResult.Pass, context);
        }

        if (leftValue == null || rightValue == null)
        {
            return (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));
        }

        foreach (var c in Comparisons)
        {
            if (!c.CanCompare(leftValue.GetType(), rightValue.GetType()))
                continue;

            var (result, newContext) = c.Compare(context, leftValue, rightValue);
            context = newContext;

            if (result != ComparisonResult.Inconclusive)
                return (result, context);
        }

        return (ComparisonResult.Inconclusive, context);
    }
}
