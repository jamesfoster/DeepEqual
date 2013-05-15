namespace DeepEquals
{
	using System.Collections.Generic;

	public interface IComparisonContext
	{
		List<Difference> Differences { get; }
		string Breadcrumb { get; }

		void AddDifference(Difference difference);
		void AddDifference(object value1, object value2);
		void AddDifference(object value1, object value2, string childProperty);

		IComparisonContext VisitingProperty(string propertyName);
		IComparisonContext VisitingIndex(string index);
		IComparisonContext VisitingIndex(int index);
	}
}