namespace DeepEqual;

public class DefaultComparison : IComparison
{
	public List<Type> SkippedTypes { get; set; }

	public DefaultComparison()
	{
		SkippedTypes = new List<Type>();
	}

	public bool CanCompare(Type type1, Type type2)
	{
		return !IsSkipped(type1) && !IsSkipped(type2)
			&& !ReflectionCache.IsValueTypeWithReferenceFields(type1)
			&& !ReflectionCache.IsValueTypeWithReferenceFields(type2);
	}

	public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
	{
		var type1 = value1.GetType();
		var type2 = value2.GetType();

		if (IsSkipped(type1) || IsSkipped(type2))
		{
			return (ComparisonResult.Inconclusive, context);
		}

		if (type1 != type2)
		{
			if (CoerceValues(ref value1, ref value2))
			{
				type1 = value1.GetType();
				type2 = value2.GetType();

				if (type1 != type2)
				{
					return (ComparisonResult.Inconclusive, context);
				}
			}
		}

		if (value1.Equals(value2))
		{
			return (ComparisonResult.Pass, context);
		}

		if (ReflectionCache.IsValueType(type1))
		{
			return (ComparisonResult.Fail, context.AddDifference(value1, value2));
		}

		return (ComparisonResult.Inconclusive, context);
	}

	private bool IsSkipped(Type type)
	{
		return SkippedTypes.Any(t => t.IsAssignableFrom(type));
	}

	public void Skip<T>()
	{
		SkippedTypes.Add(typeof (T));
	}

	private static bool CoerceValues(ref object value1, ref object value2)
	{
		try
		{
			value2 = Convert.ChangeType(value2, value1.GetType());
			return true;
		}
		catch{}

		try
		{
			value1 = Convert.ChangeType(value1, value2.GetType());
			return true;
		}
		catch{}

		return
			CallImplicitOperator(ref value2, value1.GetType()) ||
			CallImplicitOperator(ref value1, value2.GetType());
	}

	private static bool CallImplicitOperator(ref object value, Type destType)
	{
		// TODO: Use ReflectionCache

		var type = value.GetType();

		var conversionMethod = type
			.GetMethods()
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

		value = conversionMethod.Invoke(null, new[] {value});
		return true;
	}
}