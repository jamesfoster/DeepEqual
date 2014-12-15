namespace DeepEqual
{
	using System.Collections.Generic;
	using System.Globalization;

	public class ComparisonContext : IComparisonContext
	{
		public ComparedObjectHash ComparedObjectHash { get; private set; }
		public List<Difference> Differences { get; private set; }
		public string Breadcrumb { get; private set; }

		public ComparisonContext() : this(string.Empty) {}

		public ComparisonContext(string breadcrumb) : this(null, breadcrumb) {}

		public ComparisonContext(List<Difference> differences, string breadcrumb) : this(differences, breadcrumb, null) {}

		public ComparisonContext(List<Difference> differences, string breadcrumb, ComparedObjectHash hash)
		{
			ComparedObjectHash = hash ?? new ComparedObjectHash();
			Differences = differences ?? new List<Difference>();
			Breadcrumb = breadcrumb;
		}

		public void AddDifference(Difference difference)
		{
			Differences.Add(difference);
		}

		public bool RecursionProtection
		{
			get { return ComparedObjectHash.Enabled; }
			set { ComparedObjectHash.Enabled = value; }
		}

		public bool ShouldVisitObjects(object item1, object item2)
		{
			return ComparedObjectHash.Add(item1, item2);
		}

		public IComparisonContext VisitingProperty(string propertyName)
		{
			var newBreadcrumb = string.Format("{0}.{1}", Breadcrumb, propertyName);

			return new ComparisonContext(Differences, newBreadcrumb, ComparedObjectHash);
		}

		public IComparisonContext VisitingIndex(object index)
		{
			var newBreadcrumb = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", Breadcrumb, index);

			return new ComparisonContext(Differences, newBreadcrumb, ComparedObjectHash);
		}

		public override string ToString()
		{
			return string.Format("Breadcrumb: {0}, Differences: {1}", Breadcrumb, Differences.Count);
		}
	}
}