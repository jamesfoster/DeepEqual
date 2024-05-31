using System;

namespace DeepEqual.Test.Helper;

public class EchoComparison : IComparison
{
    private readonly ComparisonResult result;

    public EchoComparison(ComparisonResult result)
    {
        this.result = result;
    }

    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        return true;
    }

    public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object leftValue, object rightValue)
    {
        return (result, context);
    }
}