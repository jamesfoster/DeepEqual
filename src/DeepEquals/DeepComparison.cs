namespace DeepEquals
{
	using System.Collections;

	public class DeepComparison
	{
		public IEqualityComparer ToDeepComparer()
		{
			var rootComparison = new CompositeComparison();

			rootComparison.AddRange(
				new DefaultComparison(),
				new EnumComparison(),
				new ListComparison(rootComparison),
				new ComplexObjectComparison(rootComparison));

			return new ComparisonComparer(rootComparison);
		}
	}
}
