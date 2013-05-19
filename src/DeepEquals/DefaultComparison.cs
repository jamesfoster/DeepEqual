namespace DeepEquals
{
	using System;

	public class DefaultComparison : IComparison
	{
		public bool CanCompare(Type type1, Type type2)
		{
			return type1 == type2;
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			if (value1.Equals(value2))
				return ComparisonResult.Pass;

			if (ReflectionCache.IsValueType(value1.GetType()))
			{
				context.AddDifference(value1, value2);
				return ComparisonResult.Fail;
			}

			return ComparisonResult.Inconclusive;
		}
	}
}