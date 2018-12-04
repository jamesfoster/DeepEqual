using System;
using System.Collections.Generic;

namespace DeepEqual.Test.Helper
{
	public class MockComparison : IComparison
	{
		private Func<Type, Type, bool> canCompare
			= (t1, t2) => true;

		private Func<IComparisonContext, object, object, (ComparisonResult, IComparisonContext)> compare
			= (c, v1, v2) => v1.Equals(v2)
				? (ComparisonResult.Pass, c)
				: (ComparisonResult.Fail, c.AddDifference(v1, v2));

		public List<(Type type1, Type type2)> CanCompareCalls { get; } = new List<(Type, Type)>();
		public List<(IComparisonContext context, object value1, object value2)> CompareCalls { get; } = new List<(IComparisonContext, object, object)>();

		public bool CanCompare(Type type1, Type type2)
		{
			CanCompareCalls.Add((type1, type2));
			return canCompare(type1, type2);
		}

		public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
		{
			CompareCalls.Add((context, value1, value2));
			return compare(context, value1, value2);
		}

		public void SetCanCompare(Func<Type, Type, bool> func) => canCompare = func;
		public void SetCompare(Func<IComparisonContext, object, object, (ComparisonResult, IComparisonContext)> func) => compare = func;
	}
}