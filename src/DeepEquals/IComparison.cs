namespace DeepEquals
{
	using System;

	public interface IComparison
	{
		bool CanCompare(Type type1, Type type2);
		ComparisonResult Compare(IComparisonContext context, object value1, object value2);
	}
}