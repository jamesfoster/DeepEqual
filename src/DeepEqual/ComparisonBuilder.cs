namespace DeepEqual
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	public class ComparisonBuilder : IComparisonBuilder<ComparisonBuilder>
	{
		public IList<IComparison> CustomComparisons { get; set; }

		protected CompositeComparison Root { get; set; }

		public ComplexObjectComparison ComplexObjectComparison { get; set; }
		public DefaultComparison DefaultComparison { get; set; }

		public static Func<IComparisonBuilder<ComparisonBuilder>> Get { get; set; } = () => new ComparisonBuilder();

		public ComparisonBuilder()
		{
			CustomComparisons = new List<IComparison>();

			Root = new CompositeComparison();

			ComplexObjectComparison = new ComplexObjectComparison(Root);
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

		public ComparisonBuilder IgnoreProperty(Func<PropertyReader, bool> func)
		{
			ComplexObjectComparison.IgnoreProperty(func);

			return this;
		}

		public ComparisonBuilder SkipDefault<T>()
		{
			DefaultComparison.Skip<T>();
			return this;
		}

		public ComparisonBuilder ExposeInternalsOf<T>()
		{
			return ExposeInternalsOf(typeof(T));
		}

		public ComparisonBuilder ExposeInternalsOf(params Type[] types)
		{
			ReflectionCache.CachePrivatePropertiesOfTypes(types);
			return this;
		}
	}
}