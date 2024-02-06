namespace DeepEqual;

public class ComplexObjectComparison : IComparison
{
	public IComparison Inner { get; set; }

	public bool IgnoreUnmatchedProperties { get; set; }

	public List<Func<PropertyReader, bool>> IgnoredProperties { get; set; }

	public ComplexObjectComparison(IComparison inner)
	{
		Inner = inner;
		IgnoredProperties = new List<Func<PropertyReader, bool>>();
	}

	public bool CanCompare(Type type1, Type type2)
	{
		return (type1.IsClass && type2.IsClass)
			|| ReflectionCache.IsValueTypeWithReferenceFields(type1)
			|| ReflectionCache.IsValueTypeWithReferenceFields(type2);
	}

	public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
	{
		var comparer = new ComplexObjectComparer(Inner, IgnoreUnmatchedProperties, IgnoredProperties);

		return comparer.CompareObjects(context, value1, value2);
	}

	public void IgnoreProperty<T>(Expression<Func<T, object>> property)
	{
		var exp = property.Body;

		if (exp is UnaryExpression cast)
		{
			exp = cast.Operand; // implicit cast to object
		}

		if (exp is MemberExpression member)
		{
			var propertyName = member.Member.Name;

			IgnoreProperty(typeof(T), propertyName);
		}
	}

	private void IgnoreProperty(Type type, string propertyName)
	{
		IgnoreProperty(property => type.IsAssignableFrom(property.DeclaringType) && property.Name == propertyName);
	}

	public void IgnoreProperty(Func<PropertyReader, bool> item)
	{
		IgnoredProperties.Add(item);
	}
}
