namespace DeepEqual;

public interface IComparison
{
    bool CanCompare(IComparisonContext context, Type leftType, Type rightType);

    (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    );
}
