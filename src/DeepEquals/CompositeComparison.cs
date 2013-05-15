namespace DeepEquals
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class CompositeComparison : IComparison
	{
		public List<IComparison> Comparisons { get; set; }

		public CompositeComparison() : this(Enumerable.Empty<IComparison>()) {}

		public CompositeComparison(IEnumerable<IComparison> comparers)
		{
			Comparisons = comparers.ToList();
		}

		public void Add(IComparison comparison)
		{
			Comparisons.Add(comparison);
		}

		public void AddRange(params IComparison[] comparisons)
		{
			Comparisons.AddRange(comparisons);
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return true;
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			foreach (var c in Comparisons)
			{
				if(!c.CanCompare(value1.GetType(), value2.GetType()))
					continue;

				var result = c.Compare(context, value1, value2);

				if (result != ComparisonResult.Inconclusive)
					return result;
			}

			return ComparisonResult.Inconclusive;
		}
	}
}