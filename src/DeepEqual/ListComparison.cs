namespace DeepEqual
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class ListComparison : IComparison
	{
		public IComparison Inner { get; }

		public ListComparison(IComparison inner)
		{
			Inner = inner;
		}

		public bool CanCompare(Type type1, Type type2)
		{
			if (!ReflectionCache.IsListType(type1) || !ReflectionCache.IsListType(type2))
				return false;

			return CheckInnerCanCompare(type1, type2);
		}

		public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
		{
			var list1 = ((IEnumerable) value1).Cast<object>().ToArray();
			var list2 = ((IEnumerable) value2).Cast<object>().ToArray();

			if (list1.Length != list2.Length)
			{
				return (ComparisonResult.Fail, context.AddDifference(list1.Length, list2.Length, "Count"));
			}

			if (list1.Length == 0)
			{
				return (ComparisonResult.Pass, context);
			}

			var zip = list1.Zip(list2, Tuple.Create).ToArray();

			var results = new List<ComparisonResult>();
			var i = 0;
			foreach (var p in zip)
			{
				var (result, innerContext) = Inner.Compare(context.VisitingIndex(i++), p.Item1, p.Item2);

				results.Add(result);
				context = context.MergeDifferencesFrom(innerContext);
			}

			return (results.ToResult(), context);
		}

		private bool CheckInnerCanCompare(Type listType1, Type listType2)
		{
			var type1 = ReflectionCache.GetEnumerationType(listType1);
			var type2 = ReflectionCache.GetEnumerationType(listType2);

			return Inner.CanCompare(type1, type2);
		}
	}
}