namespace DeepEqual
{
	using System.Collections.Immutable;

	public interface IComparisonContext
	{
		ImmutableList<Difference> Differences { get; }
		string Breadcrumb { get; }

		IComparisonContext AddDifference(Difference difference);
		IComparisonContext SetBreadcrumb(string breadcrumb);

		IComparisonContext VisitingProperty(string propertyName);
		IComparisonContext VisitingIndex(object index);
	}
}