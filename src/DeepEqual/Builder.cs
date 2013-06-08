namespace DeepEqual
{
	using System.Collections.Generic;
	using System.Linq;

	public class Builder
	{
		public IList<IComparison> CustomComparisons { get; set; }

		public bool UnmatchedPropertiesIgnored { get; set; }

		public Builder()
		{
			CustomComparisons = new List<IComparison>();
		}

		public CompositeComparison Create()
		{
			var root = new CompositeComparison();

			root.AddRange(CustomComparisons.ToArray());

			root.AddRange(
				new DefaultComparison(),
				new EnumComparison(),
				new DictionaryComparison(new DefaultComparison(), root),
				new SetComparison(root),
				new ListComparison(root),
				new ComplexObjectComparison(root)
					{
						IgnoreUnmatchedProperties = UnmatchedPropertiesIgnored
					}
				);

			return root;
		}

		public Builder IgnoreUnmatchedProperties()
		{
			UnmatchedPropertiesIgnored = true;

			return this;
		}

		public Builder WithCustomComparison(IComparison comparison)
		{
			CustomComparisons.Add(comparison);

			return this;
		}
	}
}