namespace DeepEqual.Test
{
	using System;

	using Moq;

	using Xbehave;

	using Shouldly;

	public class ComparisonBuilderTests
	{
		protected ComparisonBuilder SUT { get; set; }

		[Scenario]
		public void Creating_a_builder()
		{
			"When creating a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"Then there should be no custom comparisons".x(() => 
				SUT.CustomComparisons.ShouldBeEmpty()
			);

			"And UnmatchedPropertiesIgnored should be false".x(() => 
				SUT.ComplexObjectComparison.IgnoreUnmatchedProperties.ShouldBe(false)
			);
		}
		
		[Scenario]
		public void Adding_a_custom_Comparison()
		{
			var result = default (ComparisonBuilder);
			var custom = default (IComparison);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"And a custom comparison".x(() => 
				custom = new Mock<IComparison>().Object
			);

			"When adding the custom comparison".x(() => 
				result = SUT.WithCustomComparison(custom)
			);

			"Then there should be a custom comparison".x(() => 
				SUT.CustomComparisons.Count.ShouldBe(1)
			);

			"And it should be the correct comparison".x(() => 
				SUT.CustomComparisons[0].ShouldBeSameAs(custom)
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}
		
		[Scenario]
		public void Ignoring_unmatched_properties()
		{
			var result = default (ComparisonBuilder);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When ignoring unmatched properties".x(() => 
				result = SUT.IgnoreUnmatchedProperties()
			);

			"Then UnmatchedPropertiesIgnored should be true".x(() => 
				SUT.ComplexObjectComparison.IgnoreUnmatchedProperties.ShouldBe(true)
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}
		
		[Scenario]
		public void Ignoring_specific_properties()
		{
			var result = default (ComparisonBuilder);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When ignoring the Major property of Version".x(() => 
				result = SUT.IgnoreProperty<Version>(x => x.Major)
			);

			"Then it should add an IgnoredProperty".x(() => 
				SUT.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1)
			);

			"And it should return true for the Major property of the Version type".x(() => 
				SUT.ComplexObjectComparison.IgnoredProperties[0](new PropertyReader
					{
						DeclaringType = typeof(Version),
						Name = "Major"
					}).ShouldBe(true)
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}
		
		[Scenario]
		public void Ignoring_specific_properties_2()
		{
			var result = default (ComparisonBuilder);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When ignoring the Major property of Version".x(() => 
				result = SUT.IgnoreProperty(x => x.Name == "Major")
			);

			"Then it should add an IgnoredProperty".x(() => 
				SUT.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1)
			);

			"And it should return true for the Major property of the Version type".x(() => 
				SUT.ComplexObjectComparison.IgnoredProperties[0](new PropertyReader
					{
						Name = "Major"
					}).ShouldBe(true)
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}
		
		[Scenario]
		public void Skipping_default_comparison_for_types()
		{
			var result = default (ComparisonBuilder);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When skipping the default comparison fer Version".x(() => 
				result = SUT.SkipDefault<Version>()
			);

			"Then SkippedTypes should contain Version".x(() => 
				SUT.DefaultComparison.SkippedTypes.ShouldContain(typeof (Version))
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}
		
		[Scenario]
		public void Expoising_internals_of_a_given_type()
		{
			var result = default (ComparisonBuilder);
			var properties = default (PropertyReader[]);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When exposing the internals of Version".x(() =>
			{
				result = SUT.ExposeInternalsOf<Version>();
				properties = ReflectionCache.GetProperties(new Version(1, 2, 3, 4));
			});

			"Then ReflectionCache should contain the private properties of Version".x(() => 
				properties.ShouldContain(x => x.Name == "_Major")
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}
		
		[Scenario]
		public void Expoising_internals_of_many_types()
		{
			var result = default (ComparisonBuilder);
			var versionProperties = default (PropertyReader[]);
			var uriProperties = default (PropertyReader[]);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When exposing the internals of Version".x(() =>
			{
				result = SUT.ExposeInternalsOf(typeof(Version), typeof(Uri));
				versionProperties = ReflectionCache.GetProperties(new Version(1, 2, 3, 4));
				uriProperties = ReflectionCache.GetProperties(new Uri("http://google.com"));
			});

			"Then ReflectionCache should contain the private properties of Version".x(() => 
				versionProperties.ShouldContain(x => x.Name == "_Major")
			);

			"And ReflectionCache should contain the private properties of Uri".x(() => 
				uriProperties.ShouldContain(x => x.Name == "_syntax")
			);

			"And it should return the builder".x(() => 
				result.ShouldBeSameAs(SUT)
			);
		}

		[Scenario]
		public void Creating_a_default_Comparison()
		{
			var result = default (CompositeComparison);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"When calling Create".x(() => 
				result = SUT.Create()
			);

			"Then it not return null".x(() => 
				result.ShouldNotBe(null)
			);

			"And the 1st comparer is the DefaultComparison".x(() => 
				result.Comparisons[0].ShouldBeAssignableTo<DefaultComparison>()
			);

			"And the 2nd comparer is the EnumComparison".x(() => 
				result.Comparisons[1].ShouldBeAssignableTo<EnumComparison>()
			);
			
			"And the 3rd comparer is the DictionaryComparison".x(() => 
				result.Comparisons[2].ShouldBeAssignableTo<DictionaryComparison>()
			);
			
			"... with a DefaultComparison as the key comparer".x(() => 
				((DictionaryComparison)result.Comparisons[2]).KeyComparer.ShouldBeAssignableTo<DefaultComparison>()
			);
			
			"... and the value comparer is the result".x(() => 
				((DictionaryComparison)result.Comparisons[2]).ValueComparer.ShouldBeSameAs(result)
			);
			
			"And the 4th comparer is the DictionaryComparison".x(() => 
				result.Comparisons[3].ShouldBeAssignableTo<SetComparison>()
			);
			
			"... and the inner comparer is the result".x(() => 
				((SetComparison)result.Comparisons[3]).Inner.ShouldBeSameAs(result)
			);
			
			"And the 5th comparer is the DictionaryComparison".x(() => 
				result.Comparisons[4].ShouldBeAssignableTo<ListComparison>()
			);
			
			"... and the inner comparer is the result".x(() => 
				((ListComparison)result.Comparisons[4]).Inner.ShouldBeSameAs(result)
			);
			
			"And the 6th comparer is the ComplexObjectComparison".x(() => 
				result.Comparisons[5].ShouldBeAssignableTo<ComplexObjectComparison>()
			);
			
			"... and the inner comparer is the result".x(() => 
				((ComplexObjectComparison)result.Comparisons[5]).Inner.ShouldBeSameAs(result)
			);
			
			"... and IgnoreUnmatchedProperties should be false".x(() => 
				((ComplexObjectComparison)result.Comparisons[5]).IgnoreUnmatchedProperties.ShouldBe(false)
			);
		}

		[Scenario]
		public void Creating_a_Comparison_with_custom_comparisons()
		{
			var result = default (CompositeComparison);
			var custom = default (IComparison);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"And a custom comparison".x(() =>
			{
				custom = new Mock<IComparison>().Object;

				SUT.WithCustomComparison(custom);
			});

			"When calling Create".x(() => 
				result = SUT.Create()
			);

			"Then the 1st comparer is the custom comparison".x(() => 
				result.Comparisons[0].ShouldBeSameAs(custom)
			);
		}

		[Scenario]
		public void Creating_a_Comparison_and_ignoring_unmatched_properties()
		{
			var result = default (CompositeComparison);

			"Given a builder".x(() => 
				SUT = new ComparisonBuilder()
			);

			"And we call IgnoreUnmatchedProperties".x(() => 
				SUT.IgnoreUnmatchedProperties()
			);

			"When calling Create".x(() => 
				result = SUT.Create()
			);

			"Then the 6th comparer is the ComplexObjectComparison".x(() => 
				result.Comparisons[5].ShouldBeAssignableTo<ComplexObjectComparison>()
			);
			
			"... and IgnoreUnmatchedProperties should be true".x(() => 
				((ComplexObjectComparison)result.Comparisons[5]).IgnoreUnmatchedProperties.ShouldBe(true)
			);
		}
	}
}