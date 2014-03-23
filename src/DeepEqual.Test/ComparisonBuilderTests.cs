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
			"When creating a builder"
				.When(() => SUT = new ComparisonBuilder());

			"Then there should be no custom comparisons"
				.Then(() => SUT.CustomComparisons.ShouldBeEmpty());

			"And UnmatchedPropertiesIgnored should be false"
				.And(() => SUT.ComplexObjectComparison.IgnoreUnmatchedProperties.ShouldBe(false));
		}
		
		[Scenario]
		public void Adding_a_custom_Comparison()
		{
			var result = default (ComparisonBuilder);
			var custom = default (IComparison);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"And a custom comparison"
				.And(() => custom = new Mock<IComparison>().Object);

			"When adding the custom comparison"
				.When(() => result = SUT.WithCustomComparison(custom));

			"Then there should be a custom comparison"
				.Then(() => SUT.CustomComparisons.Count.ShouldBe(1));

			"And it should be the correct comparison"
				.And(() => SUT.CustomComparisons[0].ShouldBeSameAs(custom));

			"And it should return the builder"
				.And(() => result.ShouldBeSameAs(SUT));
		}
		
		[Scenario]
		public void Ignoring_unmatched_properties()
		{
			var result = default (ComparisonBuilder);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"When ignoring unmatched properties"
				.When(() => result = SUT.IgnoreUnmatchedProperties());

			"Then UnmatchedPropertiesIgnored should be true"
				.Then(() => SUT.ComplexObjectComparison.IgnoreUnmatchedProperties.ShouldBe(true));

			"And it should return the builder"
				.And(() => result.ShouldBeSameAs(SUT));
		}
		
		[Scenario]
		public void Ignoring_specific_properties()
		{
			var result = default (ComparisonBuilder);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"When ignoring the Major property of Version"
				.When(() => result = SUT.IgnoreProperty<Version>(x => x.Major));

			"Then it should add an IgnoredProperty"
				.Then(() => SUT.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1));

			"And it should return true for the Major property of the Version type"
				.Then(() => SUT.ComplexObjectComparison.IgnoredProperties[0](new PropertyReader
					{
						DeclaringType = typeof(Version),
						Name = "Major"
					}).ShouldBe(true));

			"And it should return the builder"
				.And(() => result.ShouldBeSameAs(SUT));
		}
		
		[Scenario]
		public void Skipping_default_comparison_for_types()
		{
			var result = default (ComparisonBuilder);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"When ignoring unmatched properties"
				.When(() => result = SUT.SkipDefault<Version>());

			"Then UnmatchedPropertiesIgnored should be true"
				.Then(() => SUT.DefaultComparison.SkippedTypes.ShouldContain(typeof (Version)));

			"And it should return the builder"
				.And(() => result.ShouldBeSameAs(SUT));
		}

		[Scenario]
		public void Creating_a_default_Comparison()
		{
			var result = default (CompositeComparison);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"When calling Create"
				.When(() => result = SUT.Create());

			"Then it not return null"
				.Then(() => result.ShouldNotBe(null));

			"And the 1st comparer is the DefaultComparison"
				.And(() => result.Comparisons[0].ShouldBeTypeOf<DefaultComparison>());

			"And the 2nd comparer is the EnumComparison"
				.And(() => result.Comparisons[1].ShouldBeTypeOf<EnumComparison>());
			
			"And the 3rd comparer is the DictionaryComparison"
				.And(() => result.Comparisons[2].ShouldBeTypeOf<DictionaryComparison>());
			
			"... with a DefaultComparison as the key comparer"
				.And(() => ((DictionaryComparison)result.Comparisons[2]).KeyComparer.ShouldBeTypeOf<DefaultComparison>());
			
			"... and the value comparer is the result"
				.And(() => ((DictionaryComparison)result.Comparisons[2]).ValueComparer.ShouldBeSameAs(result));
			
			"And the 4th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[3].ShouldBeTypeOf<SetComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((SetComparison)result.Comparisons[3]).Inner.ShouldBeSameAs(result));
			
			"And the 5th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[4].ShouldBeTypeOf<ListComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((ListComparison)result.Comparisons[4]).Inner.ShouldBeSameAs(result));
			
			"And the 6th comparer is the ComplexObjectComparison"
				.And(() => result.Comparisons[5].ShouldBeTypeOf<ComplexObjectComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((ComplexObjectComparison)result.Comparisons[5]).Inner.ShouldBeSameAs(result));
			
			"... and IgnoreUnmatchedProperties should be false"
				.And(() => ((ComplexObjectComparison)result.Comparisons[5]).IgnoreUnmatchedProperties.ShouldBe(false));
		}

		[Scenario]
		public void Creating_a_Comparison_with_custom_comparisons()
		{
			var result = default (CompositeComparison);
			var custom = default (IComparison);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"And a custom comparison"
				.And(() =>
					{
						custom = new Mock<IComparison>().Object;

						SUT.WithCustomComparison(custom);
					});

			"When calling Create"
				.When(() => result = SUT.Create());

			"Then it not return null"
				.Then(() => result.ShouldNotBe(null));

			"And the 1st comparer is the custom comparison"
				.And(() => result.Comparisons[0].ShouldBeSameAs(custom));

			"And the 2nd comparer is the DefaultComparison"
				.And(() => result.Comparisons[1].ShouldBeTypeOf<DefaultComparison>());

			"And the 3rd comparer is the EnumComparison"
				.And(() => result.Comparisons[2].ShouldBeTypeOf<EnumComparison>());
			
			"And the 4th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[3].ShouldBeTypeOf<DictionaryComparison>());
			
			"... with a DefaultComparison as the key comparer"
				.And(() => ((DictionaryComparison)result.Comparisons[3]).KeyComparer.ShouldBeTypeOf<DefaultComparison>());
			
			"... and the value comparer is the result"
				.And(() => ((DictionaryComparison)result.Comparisons[3]).ValueComparer.ShouldBeSameAs(result));
			
			"And the 5th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[4].ShouldBeTypeOf<SetComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((SetComparison)result.Comparisons[4]).Inner.ShouldBeSameAs(result));
			
			"And the 6th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[5].ShouldBeTypeOf<ListComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((ListComparison)result.Comparisons[5]).Inner.ShouldBeSameAs(result));
			
			"And the 7th comparer is the ComplexObjectComparison"
				.And(() => result.Comparisons[6].ShouldBeTypeOf<ComplexObjectComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((ComplexObjectComparison)result.Comparisons[6]).Inner.ShouldBeSameAs(result));
			
			"... and IgnoreUnmatchedProperties should be false"
				.And(() => ((ComplexObjectComparison)result.Comparisons[6]).IgnoreUnmatchedProperties.ShouldBe(false));
		}

		[Scenario]
		public void Creating_a_Comparison_and_ignoring_unmatched_properties()
		{
			var result = default (CompositeComparison);

			"Given a builder"
				.Given(() => SUT = new ComparisonBuilder());

			"And we call IgnoreUnmatchedProperties"
				.And(() => SUT.IgnoreUnmatchedProperties());

			"When calling Create"
				.When(() => result = SUT.Create());

			"Then it not return null"
				.Then(() => result.ShouldNotBe(null));

			"And the 1st comparer is the DefaultComparison"
				.And(() => result.Comparisons[0].ShouldBeTypeOf<DefaultComparison>());

			"And the 2nd comparer is the EnumComparison"
				.And(() => result.Comparisons[1].ShouldBeTypeOf<EnumComparison>());
			
			"And the 3rd comparer is the DictionaryComparison"
				.And(() => result.Comparisons[2].ShouldBeTypeOf<DictionaryComparison>());
			
			"... with a DefaultComparison as the key comparer"
				.And(() => ((DictionaryComparison)result.Comparisons[2]).KeyComparer.ShouldBeTypeOf<DefaultComparison>());
			
			"... and the value comparer is the result"
				.And(() => ((DictionaryComparison)result.Comparisons[2]).ValueComparer.ShouldBeSameAs(result));
			
			"And the 4th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[3].ShouldBeTypeOf<SetComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((SetComparison)result.Comparisons[3]).Inner.ShouldBeSameAs(result));
			
			"And the 5th comparer is the DictionaryComparison"
				.And(() => result.Comparisons[4].ShouldBeTypeOf<ListComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((ListComparison)result.Comparisons[4]).Inner.ShouldBeSameAs(result));
			
			"And the 6th comparer is the ComplexObjectComparison"
				.And(() => result.Comparisons[5].ShouldBeTypeOf<ComplexObjectComparison>());
			
			"... and the inner comparer is the result"
				.And(() => ((ComplexObjectComparison)result.Comparisons[5]).Inner.ShouldBeSameAs(result));
			
			"... and IgnoreUnmatchedProperties should be true"
				.And(() => ((ComplexObjectComparison)result.Comparisons[5]).IgnoreUnmatchedProperties.ShouldBe(true));
		}
	}
}