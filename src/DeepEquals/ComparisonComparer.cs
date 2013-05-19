namespace DeepEquals
{
	using System.Collections;
	using System.Collections.Generic;

	public class ComparisonComparer : IEqualityComparer, IEqualityComparer<object>
	{
		private readonly IComparison RootComparison;

		public ComparisonComparer(IComparison rootComparison)
		{
			RootComparison = rootComparison;
		}

		public new bool Equals(object x, object y)
		{
			var context = new ComparisonContext();

			var result = RootComparison.Compare(context, x, y);

			return result == ComparisonResult.Pass;
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}