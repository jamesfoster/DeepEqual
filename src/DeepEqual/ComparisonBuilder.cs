namespace DeepEqual
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	public class ComparisonBuilder
	{
		public IList<IComparison> CustomComparisons { get; set; }

		protected CompositeComparison Root { get; set; }

		public ComplexObjectComparison ComplexObjectComparison { get; set; }

		public ComparisonBuilder()
		{
			CustomComparisons = new List<IComparison>();

			Root = new CompositeComparison();

			ComplexObjectComparison = new ComplexObjectComparison(Root);
		}

		public CompositeComparison Create()
		{
			Root.AddRange(CustomComparisons.ToArray());

			Root.AddRange(
				new DefaultComparison(),
				new EnumComparison(),
				new DictionaryComparison(new DefaultComparison(), Root),
				new SetComparison(Root),
				new ListComparison(Root),
				ComplexObjectComparison
				);

			return Root;
		}

		public ComparisonBuilder IgnoreUnmatchedProperties()
		{
			ComplexObjectComparison.IgnoreUnmatchedProperties = true;

			return this;
		}

		public ComparisonBuilder WithCustomComparison(IComparison comparison)
		{
			CustomComparisons.Add(comparison);

			return this;
		}

		public ComparisonBuilder IgnoreProperty<T>(Expression<Func<T, object>> property)
		{
			ComplexObjectComparison.IgnoreProperty(property);

			return this;
		}
	}
}