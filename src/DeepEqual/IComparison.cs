namespace DeepEqual
{
	using System;

	public interface IComparison
	{
		bool CanCompare(Type type1, Type type2);
		(ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2);
	}
}