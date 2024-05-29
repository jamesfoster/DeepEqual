namespace DeepEqual;

public class EnumComparison : IComparison
{
    public bool CanCompare(Type leftType, Type rightType)
    {
        if (!leftType.IsEnum && !rightType.IsEnum)
            return false;

        return (leftType.IsEnum || leftType == typeof(string) || leftType == typeof(int))
            && (rightType.IsEnum || rightType == typeof(string) || rightType == typeof(int));
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

        var leftValueIsEnum = leftValue.GetType().IsEnum;
        var rightValueIsEnum = rightValue.GetType().IsEnum;

        if (leftValueIsEnum && rightValueIsEnum)
        {
            return leftValue.ToString() == rightValue.ToString()
                ? (ComparisonResult.Pass, context)
                : (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));
        }

        var areEqual = leftValueIsEnum
            ? CompareEnumWithConversion(leftValue, rightValue)
            : CompareEnumWithConversion(rightValue, leftValue);

        return areEqual
            ? (ComparisonResult.Pass, context)
            : (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));
    }

    private static bool CompareEnumWithConversion(object leftValue, object rightValue)
    {
        var type = leftValue.GetType();

        try
        {
            if (rightValue is int i)
                rightValue = Enum.ToObject(type, i);

            if (rightValue is string s)
                rightValue = Enum.Parse(type, s);
        }
        catch (Exception)
        {
            return false;
        }

        return leftValue.Equals(rightValue);
    }
}
