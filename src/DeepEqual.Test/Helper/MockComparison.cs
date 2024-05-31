using System;
using System.Collections.Generic;

namespace DeepEqual.Test.Helper;

public class MockComparison : IComparison
{
    private Func<IComparisonContext, Type, Type, bool> canCompare
        = (c, t1, t2) => true;

    private Func<IComparisonContext, object, object, (ComparisonResult, IComparisonContext)> compare
        = (c, v1, v2) => v1.Equals(v2)
            ? (ComparisonResult.Pass, c)
            : (ComparisonResult.Fail, c.AddDifference(v1, v2));

    public List<(IComparisonContext context, Type leftType, Type rightType)> CanCompareCalls { get; } = new List<(IComparisonContext, Type, Type)>();
    public List<(IComparisonContext context, object leftValue, object rightValue)> CompareCalls { get; }
        = new List<(IComparisonContext, object, object)>();

    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        CanCompareCalls.Add((context, leftType, rightType));
        return canCompare(context, leftType, rightType);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object leftValue,
        object rightValue
    )
    {
        CompareCalls.Add((context, leftValue, rightValue));
        return compare(context, leftValue, rightValue);
    }

    public void SetCanCompare(Func<IComparisonContext, Type, Type, bool> func)
    {
        canCompare = func;
    }

    public void SetCompare(
        Func<IComparisonContext, object, object, (ComparisonResult, IComparisonContext)> func
    )
    {
        compare = func;
    }
}