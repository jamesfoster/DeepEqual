namespace DeepEqual;

public class DefaultComparison : IComparison
{
    public List<Type> SkippedTypes { get; set; }

    public DefaultComparison()
    {
        SkippedTypes = new List<Type>();
    }

    public bool CanCompare(Type leftType, Type rightType)
    {
        return !IsSkipped(leftType)
            && !IsSkipped(rightType)
            && !ReflectionCache.IsValueTypeWithReferenceFields(leftType)
            && !ReflectionCache.IsValueTypeWithReferenceFields(rightType);
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

        var leftType = leftValue.GetType();
        var rightType = rightValue.GetType();

        if (IsSkipped(leftType) || IsSkipped(rightType))
        {
            return (ComparisonResult.Inconclusive, context);
        }

        if (leftType != rightType)
        {
            if (CoerceValues(ref leftValue, ref rightValue))
            {
                leftType = leftValue.GetType();
                rightType = rightValue.GetType();

                if (leftType != rightType)
                {
                    return (ComparisonResult.Inconclusive, context);
                }
            }
        }

        if (leftValue.Equals(rightValue))
        {
            return (ComparisonResult.Pass, context);
        }

        if (ReflectionCache.IsValueType(leftType))
        {
            return (ComparisonResult.Fail, context.AddDifference(leftValue, rightValue));
        }

        return (ComparisonResult.Inconclusive, context);
    }

    private bool IsSkipped(Type type)
    {
        return SkippedTypes.Any(t => t.IsAssignableFrom(type));
    }

    public void Skip<T>()
    {
        SkippedTypes.Add(typeof(T));
    }

    private static bool CoerceValues(ref object leftValue, ref object rightValue)
    {
        try
        {
            rightValue = Convert.ChangeType(rightValue, leftValue.GetType());
            return true;
        }
        catch { }

        try
        {
            leftValue = Convert.ChangeType(leftValue, rightValue.GetType());
            return true;
        }
        catch { }

        return CallImplicitOperator(ref rightValue, leftValue.GetType())
            || CallImplicitOperator(ref leftValue, rightValue.GetType());
    }

    private static bool CallImplicitOperator(ref object value, Type destType)
    {
        // TODO: Use ReflectionCache

        var type = value.GetType();

        var conversionMethod = type.GetMethods()
            .Where(x => x.Name == "op_Implicit" || x.Name == "op_Explicit")
            .FirstOrDefault(x => x.ReturnType == destType);

        if (conversionMethod == null)
        {
            conversionMethod = destType
                .GetMethods()
                .Where(x => x.Name == "op_Implicit" || x.Name == "op_Explicit")
                .FirstOrDefault(x => x.GetParameters().First().ParameterType == type);
        }

        if (conversionMethod == null)
            return false;

        value = conversionMethod.Invoke(null, new[] { value })!;
        return true;
    }
}
