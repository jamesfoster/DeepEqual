using System;
using System.Collections.Generic;

namespace DeepEqual.Test.Helper;

public class MockComparison : IComparison
{
    private Func<Type, Type, bool> canCompare
        = (t1, t2) => true;

    private Func<IComparisonContext, object, object, (ComparisonResult, IComparisonContext)> compare
        = (c, v1, v2) => v1.Equals(v2)
            ? (ComparisonResult.Pass, c)
            : (ComparisonResult.Fail, c.AddDifference(v1, v2));

    public List<(Type leftType, Type rightType)> CanCompareCalls { get; } = new List<(Type, Type)>();
    public List<(IComparisonContext context, object leftValue, object rightValue)> CompareCalls { get; } = new List<(IComparisonContext, object, object)>();

    public bool CanCompare(Type leftType, Type rightType)
    {
        CanCompareCalls.Add((leftType, rightType));
        return canCompare(leftType, rightType);
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

    public void SetCanCompare(Func<Type, Type, bool> func) => canCompare = func;
    public void SetCompare(Func<IComparisonContext, object, object, (ComparisonResult, IComparisonContext)> func) => compare = func;
}