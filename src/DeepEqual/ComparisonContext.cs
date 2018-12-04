namespace DeepEqual
{
	using System.Collections.Generic;
	using System.Globalization;

	public class ComparisonContext : IComparisonContext
	{
		public List<Difference> Differences { get; }
		public string Breadcrumb { get; }

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

		public IComparisonContext VisitingProperty(string propertyName)
		{
			var newBreadcrumb = $"{Breadcrumb}.{propertyName}";

			return new ComparisonContext(Differences, newBreadcrumb);
		}

		public IComparisonContext VisitingIndex(object index)
		{
			var newBreadcrumb = $"{Breadcrumb}[{index}]";

			return new ComparisonContext(Differences, newBreadcrumb);
		}
	}
}