namespace DeepEqual
{
	using System.Collections.Generic;

	public interface IComparisonContext
	{
		List<Difference> Differences { get; }
		string Breadcrumb { get; }

		void AddDifference(Difference difference);

		IComparisonContext VisitingProperty(string propertyName);
		IComparisonContext VisitingIndex(object index);
		bool ShouldVisitObjects(object item1, object item2);
	}
}