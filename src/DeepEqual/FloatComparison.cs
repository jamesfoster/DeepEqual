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

	public bool CanCompare(Type type1, Type type2)
	{
		return IsFloat(type1) && IsNumber(type2)
			|| IsFloat(type2) && IsNumber(type1);
	}

	public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
	{
		if (IsDoublePrecision(value1.GetType()) || IsDoublePrecision(value2.GetType()))
			return CompareDoublePrecision(context, value1, value2);

		return CompareSinglePrecision(context, value1, value2);
	}

	private (ComparisonResult result, IComparisonContext context) CompareSinglePrecision(IComparisonContext context, object value1, object value2)
	{
		var num1 = Convert.ToSingle(value1);
		var num2 = Convert.ToSingle(value2);

			if ((float.IsNaN(num1) && float.IsNaN(num2)) || (num1 == num2))
				return (ComparisonResult.Pass, context);

			if (singleTolerance == 0.0f)
				return (ComparisonResult.Fail, context.AddDifference(value1, value2));

		var epsilon = Math.Max(Math.Abs(num1), Math.Abs(num2)) * singleTolerance;

		if (epsilon == 0.0 || Math.Abs(num1 - num2) < epsilon)
			return (ComparisonResult.Pass, context);

		return (ComparisonResult.Fail, context.AddDifference(value1, value2));
	}

	private (ComparisonResult result, IComparisonContext context) CompareDoublePrecision(IComparisonContext context, object value1, object value2)
	{
		var num1 = Convert.ToDouble(value1);
		var num2 = Convert.ToDouble(value2);

			if ((double.IsNaN(num1) && double.IsNaN(num2)) || (num1 == num2))
				return (ComparisonResult.Pass, context);

		if (doubleTolerance == 0.0f)
			return (ComparisonResult.Fail, context.AddDifference(value1, value2));

		var epsilon = Math.Max(Math.Abs(num1), Math.Abs(num2)) * doubleTolerance;

		if (epsilon == 0.0 || Math.Abs(num1 - num2) < epsilon)
			return (ComparisonResult.Pass, context);

		return (ComparisonResult.Fail, context.AddDifference(value1, value2));
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