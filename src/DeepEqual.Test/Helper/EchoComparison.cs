namespace DeepEqual.Test.Helper
{
	using System;

	public class EchoComparison : IComparison
	{
		private readonly ComparisonResult result;

		public EchoComparison(ComparisonResult result)
		{
			this.result = result;
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return true;
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			return result;
		}
	}
}