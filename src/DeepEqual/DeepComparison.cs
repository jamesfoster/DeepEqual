namespace DeepEqual
{
	using System.Collections;

	public class DeepComparison
	{
		public static IEqualityComparer CreateComparer()
		{
			return new ComparisonComparer(Create());
		}

		public static CompositeComparison Create()
		{
			return new Builder().Create();
		}
	}
}
