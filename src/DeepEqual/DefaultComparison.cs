namespace DeepEqual
{
	using System;
	using System.Collections.Generic;

	using System.Linq;

	public class DefaultComparison : IComparison
	{
		public List<Type> SkippedTypes { get; set; }

		public DefaultComparison()
		{
			SkippedTypes = new List<Type>();
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return type1 == type2 && !IsSkipped(type1);
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			var type1 = value1.GetType();

			if (IsSkipped(type1))
			{
				return ComparisonResult.Inconclusive;
			}

			if (value1.Equals(value2))
			{
				return ComparisonResult.Pass;
			}

			if (ReflectionCache.IsValueType(type1))
			{
				context.AddDifference(value1, value2);
				return ComparisonResult.Fail;
			}

			return ComparisonResult.Inconclusive;
		}

		private bool IsSkipped(Type type)
		{
			return SkippedTypes.Any(t => t.IsAssignableFrom(type));
		}

		public void Skip<T>()
		{
			SkippedTypes.Add(typeof (T));
		}
	}
}