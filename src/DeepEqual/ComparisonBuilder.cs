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
        public IgnoredProperties IgnoredProperties = new IgnoredProperties();

        public ListComparison ListComparison { get; set; }
        public ComplexObjectComparison ComplexObjectComparison { get; set; }
		public DefaultComparison DefaultComparison { get; set; }

		public ComparisonBuilder()
		{
			CustomComparisons = new List<IComparison>();

			Root = new CompositeComparison();

            ListComparison = new ListComparison(Root, IgnoredProperties);
            ComplexObjectComparison = new ComplexObjectComparison(Root, IgnoredProperties);
			DefaultComparison = new DefaultComparison();
		}

		public CompositeComparison Create()
		{
			Root.AddRange(CustomComparisons.ToArray());

			Root.AddRange(
				DefaultComparison,
				new EnumComparison(),
				new DictionaryComparison(new DefaultComparison(), Root),
				new SetComparison(Root),
                ListComparison,
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
            IgnoredProperties.IgnoreProperty(property);

			return this;   
		}

        public ComparisonBuilder IgnoreProperty(string property)
        {
            IgnoredProperties.IgnoreProperty(property);

            return this;
        }

        public ComparisonBuilder DisregardListOrder()
        {
            ListComparison.DisregardListOrder = true;

            return this;
        }

        public ComparisonBuilder SkipDefault<T>()
		{
			DefaultComparison.Skip<T>();
			return this;
		}
	}
}