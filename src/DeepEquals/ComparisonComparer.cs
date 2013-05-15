namespace DeepEquals
{
	using System.Collections;

	public class ComparisonComparer : IEqualityComparer
	{
		private readonly CompositeComparison RootComparison;

		public ComparisonComparer(CompositeComparison rootComparison)
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
			throw new System.NotImplementedException();
		}
	}
}