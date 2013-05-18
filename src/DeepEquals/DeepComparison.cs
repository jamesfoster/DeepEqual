namespace DeepEquals
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
			var root = new CompositeComparison();

			root.AddRange(
				new DefaultComparison(),
				new EnumComparison(),
				new ListComparison(root),
				new ComplexObjectComparison(root));

			return root;
		}
	}
}
