namespace DeepEqual
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class SetComparison : IComparison
	{
		public IComparison Inner { get; set; }

		public SetComparison(IComparison inner)
		{
			Inner = inner;
		}

		public bool CanCompare(Type type1, Type type2)
		{
			var isSetType1 = ReflectionCache.IsSetType(type1);
			var isSetType2 = ReflectionCache.IsSetType(type2);
			
			if (!isSetType1 && !isSetType2)
				return false;

			if (!isSetType1 && !ReflectionCache.IsListType(type1))
				return false;

			if (!isSetType2 && !ReflectionCache.IsListType(type2))
				return false;

			var elementType1 = ReflectionCache.GetEnumerationType(type1);
			var elementType2 = ReflectionCache.GetEnumerationType(type2);

			return Inner.CanCompare(elementType1, elementType2);
		}

		public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
		{
			var set1 = ((IEnumerable) value1).Cast<object>().ToArray();
			var set2 = ((IEnumerable) value2).Cast<object>().ToArray();

			if (set1.Length != set2.Length)
			{
				return (ComparisonResult.Fail, context.AddDifference(set1.Length, set2.Length, "Count"));
			}

			if (set1.Length == 0)
			{
				return (ComparisonResult.Pass, context);
			}

			return SetsEqual(context, set1, set2);
		}

		private (ComparisonResult result, IComparisonContext context) SetsEqual(IComparisonContext context, object[] set1, object[] set2)
		{
			var expected = set2.ToList();
			var extra = new List<object>();

			foreach (var obj in set1)
			{
				var innerContext = new ComparisonContext();
				var found = expected.FirstOrDefault(e => Inner.Compare(innerContext, obj, e).result == ComparisonResult.Pass);

				if (found != null)
					expected.RemoveAll(x => ReferenceEquals(x, found));
				else
					extra.Add(obj);
			}

			var equal = expected.Count == 0 && extra.Count == 0;

			if (!equal)
			{
				return (
					ComparisonResult.Fail,
					context.AddDifference(
						new SetDifference(
							context.Breadcrumb,
							expected,
							extra
						)
					)
				);
			}

			return (ComparisonResult.Pass, context);
		}
	}
}