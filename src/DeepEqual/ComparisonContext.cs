namespace DeepEqual;

public class ComparisonContext : IComparisonContext
{
	public ImmutableList<Difference> Differences { get; }
	public string Breadcrumb { get; }

	public ComparisonContext() : this(string.Empty) {}

	public ComparisonContext(string breadcrumb) : this(null, breadcrumb) {}

	public ComparisonContext(ImmutableList<Difference>? differences, string breadcrumb)
	{
		Differences = differences ?? ImmutableList<Difference>.Empty;
		Breadcrumb = breadcrumb;
	}

	public IComparisonContext AddDifference(Difference difference)
	{
		var newDifferences = Differences.Add(difference);

		return new ComparisonContext(newDifferences, Breadcrumb);
	}

	public IComparisonContext SetBreadcrumb(string breadcrumb)
	{
		return new ComparisonContext(Differences, breadcrumb);
	}
}