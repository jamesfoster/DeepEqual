namespace DeepEquals
{
	using System.Collections.Generic;
	using System.Globalization;

	public class ComparisonContext : IComparisonContext
	{
		public List<Difference> Differences { get; private set; }
		public string Breadcrumb { get; private set; }

		public ComparisonContext() : this(string.Empty) {}

		public ComparisonContext(string breadcrumb) : this(null, breadcrumb) {}

		public ComparisonContext(List<Difference> differences, string breadcrumb)
		{
			Differences = differences ?? new List<Difference>();
			Breadcrumb = breadcrumb;
		}

		public void AddDifference(Difference difference)
		{
			Differences.Add(difference);
		}

		public void AddDifference(object value1, object value2)
		{
			AddDifference(value1, value2, null);
		}

		public void AddDifference(object value1, object value2, string childProperty)
		{
			AddDifference(new Difference
				{
					Breadcrumb = Breadcrumb,
					Value1 = value1,
					Value2 = value2,
					ChildProperty = childProperty
				});
		}

		public IComparisonContext VisitingProperty(string propertyName)
		{
			var newBreadcrumb = string.Format("{0}.{1}", Breadcrumb, propertyName);

			return new ComparisonContext(Differences, newBreadcrumb);
		}

		public IComparisonContext VisitingIndex(string index)
		{
			var newBreadcrumb = string.Format("{0}[{1}]", Breadcrumb, index);

			return new ComparisonContext(Differences, newBreadcrumb);
		}

		public IComparisonContext VisitingIndex(int index)
		{
			return VisitingIndex(index.ToString(CultureInfo.InvariantCulture));
		}

		public override string ToString()
		{
			return string.Format("Breadcrumb: {0}, Differences: {1}", Breadcrumb, Differences.Count);
		}
	}
}