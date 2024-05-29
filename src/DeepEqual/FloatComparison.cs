namespace DeepEqual;

public class FloatComparison : IComparison
{
    private readonly double doubleTolerance;
    private readonly float singleTolerance;

    public FloatComparison(double doubleTolerance, float singleTolerance)
    {
        this.doubleTolerance = doubleTolerance;
        this.singleTolerance = singleTolerance;
    }

    public bool CanCompare(Type leftType, Type rightType)
    {
        return IsFloat(leftType) && IsNumber(rightType) || IsFloat(rightType) && IsNumber(leftType);
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

        if (IsDoublePrecision(leftValue.GetType()) || IsDoublePrecision(rightValue.GetType()))
            return CompareDoublePrecision(context, leftValue, rightValue);

        return CompareSinglePrecision(context, leftValue, rightValue);
    }

    private (ComparisonResult result, IComparisonContext context) CompareSinglePrecision(
        IComparisonContext context,
        object leftValue,
        object rightValue
    )
    {
        var leftNum = Convert.ToSingle(leftValue);
        var rightNum = Convert.ToSingle(rightValue);

        if ((float.IsNaN(leftNum) && float.IsNaN(rightNum)) || (leftNum == rightNum))
            return (ComparisonResult.Pass, context);

        if (singleTolerance == 0.0f)
            return (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));

        var epsilon = Math.Max(Math.Abs(leftNum), Math.Abs(rightNum)) * singleTolerance;

        if (epsilon == 0.0 || Math.Abs(leftNum - rightNum) < epsilon)
            return (ComparisonResult.Pass, context);

        return (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));
    }

    private (ComparisonResult result, IComparisonContext context) CompareDoublePrecision(
        IComparisonContext context,
        object leftValue,
        object rightValue
    )
    {
        var leftNum = Convert.ToDouble(leftValue);
        var rightNum = Convert.ToDouble(rightValue);

        if ((double.IsNaN(leftNum) && double.IsNaN(rightNum)) || (leftNum == rightNum))
            return (ComparisonResult.Pass, context);

        if (doubleTolerance == 0.0f)
            return (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));

        var epsilon = Math.Max(Math.Abs(leftNum), Math.Abs(rightNum)) * doubleTolerance;

        if (epsilon == 0.0 || Math.Abs(leftNum - rightNum) < epsilon)
            return (ComparisonResult.Pass, context);

        return (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));
    }

    private static bool IsFloat(Type type) => IsSinglePrecision(type) || IsDoublePrecision(type);

    private static bool IsSinglePrecision(Type type) => type == typeof(float);

    private static bool IsDoublePrecision(Type type) => type == typeof(double);

    private static bool IsNumber(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}
