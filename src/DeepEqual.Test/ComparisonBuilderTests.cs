using System;

using Moq;

using Xbehave;

using Shouldly;
using Xunit;

namespace DeepEqual.Test;

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
            result.CustomComparisons.Count.ShouldBe(1)
        );

        "And it should be the correct comparison".x(() =>
            result.CustomComparisons[0].ShouldBeSameAs(custom)
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
            result.ComplexObjectComparison.IgnoreUnmatchedProperties.ShouldBe(true)
        );
    }

    [Scenario]
    public void Ignoring_specific_properties()
    {
        var result = default (ComparisonBuilder);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder().IgnoreCircularReferences()
        );

        "And a clean reflection cache".x(() =>
            ReflectionCache.ClearCache()
        );

        "When ignoring the Major property of Version".x(() =>
            result = SUT.IgnoreProperty<Version>(x => x.Major)
        );

        "Then it should add an IgnoredProperty".x(() =>
            result.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1)
        );

        "And it should ignore Major property of the Version type".x(() =>
            SUT.ComplexObjectComparison.Compare(
                new ComparisonContext(),
                new Version(9, 9, 9, 9),
                new Version(1, 9, 9, 9)
            ).context.Differences.ShouldBeEmpty()
        );
    }

    [Scenario]
    public void Ignoring_specific_properties_2()
    {
        var result = default (ComparisonBuilder);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder().IgnoreCircularReferences()
        );

        "And a clean reflection cache".x(() =>
            ReflectionCache.ClearCache()
        );

        "When ignoring the Major property of Version".x(() =>
            result = SUT.IgnoreProperty(x => x.Left.Name == "Major")
        );

        "Then it should add an IgnoredProperty".x(() =>
            result.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1)
        );

        "And it should ignore Major property of the Version type".x(() =>
            SUT.ComplexObjectComparison.Compare(
                new ComparisonContext(),
                new Version(9, 9, 9, 9),
                new Version(1, 9, 9, 9)
            ).context.Differences.ShouldBeEmpty()
        );
    }

    [Scenario]
    public void Ignoring_specific_properties_if_missing_when_not_missing()
    {
        var result = default(ComparisonBuilder);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder().IgnoreCircularReferences()
        );

        "And a clean reflection cache".x(() =>
            ReflectionCache.ClearCache()
        );

        "When ignoring the Major property of Version".x(() =>
            result = SUT.IgnorePropertyIfMissing<Version>(x => x.Major)
        );

        "Then it should add an IgnoredProperty".x(() =>
            result.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1)
        );

        "And it should throw when attempting to compare if the property is not missing".x(() =>
        {
            var action = void () => SUT.ComplexObjectComparison.Compare(
                new ComparisonContext(),
                new Version(9, 9, 9, 9),
                new AlmostVersion
                {
                    Major = 1,
                    Minor = 9,
                    Revision = 9,
                    Build = 9,
                    MajorRevision = 0,
                    MinorRevision = 9
                }
            );

            var exception = Assert.Throws<ExpectedMissingProperty>(action);
            exception.Message.ShouldBe(
                "Expected property Major to be missing from" +
                " type DeepEqual.Test.ComparisonBuilderTests+AlmostVersion"
            );
        });
    }

    [Scenario]
    public void Ignoring_specific_properties_if_missing_when_missing()
    {
        var result = default(ComparisonBuilder);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder().IgnoreCircularReferences()
        );

        "And a clean reflection cache".x(() =>
            ReflectionCache.ClearCache()
        );

        "When ignoring the Major property of Version".x(() =>
            result = SUT.IgnorePropertyIfMissing<Version>(x => x.Major)
        );

        "Then it should add an IgnoredProperty".x(() =>
            result.ComplexObjectComparison.IgnoredProperties.Count.ShouldBe(1)
        );

        "And it should ignore Major property of the Version type".x(() =>
            SUT.ComplexObjectComparison.Compare(
                new ComparisonContext(),
                new Version(9, 9, 9, 9),
                new
                {
                    Minor = 9,
                    Revision = 9,
                    Build = 9,
                    MajorRevision = 0,
                    MinorRevision = 9
                }
            ).context.Differences.ShouldBeEmpty()
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
            result.DefaultComparison.SkippedTypes.ShouldContain(typeof (Version))
        );
    }

    [Scenario]
    public void Setting_floating_point_tolerance()
    {
        var result = default (ComparisonBuilder);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder()
        );

        "When setting floating point tolerance".x(() =>
            result = SUT.WithFloatingPointTolerance(0.001d, 0.2f)
        );

        "Then the tolerance should be set".x(() =>
        {
            result.DoubleTolerance.ShouldBe(0.001d);
            result.SingleTolerance.ShouldBe(0.2f);
        });
    }

    [Scenario]
    public void Default_floating_point_tolerance()
    {
        "Given a builder".x(() =>
            SUT = new ComparisonBuilder()
        );

        "Then the tolerance should be set".x(() =>
        {
            SUT.DoubleTolerance.ShouldBe(1e-15d);
            SUT.SingleTolerance.ShouldBe(1e-6f);
        });
    }

    [Scenario]
    public void Exposing_internals_of_a_given_type()
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

        "When exposing the internals of Version and Uri".x(() =>
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
    }

    [Scenario]
    public void Creating_a_default_Comparison()
    {
        var root = default (IComparison);
        var result = default (CompositeComparison);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder()
        );

        "When calling Create".x(() =>
        {
            root = SUT.Create();
            result = ((CycleGuard)root).Inner as CompositeComparison;
        });

        "Then it not return null".x(() =>
            result.ShouldNotBe(null)
        );

        "And the 1st comparer is the DefaultComparison".x(() =>
            result.Comparisons[0].ShouldBeAssignableTo<FloatComparison>()
        );

        "And the 2nd comparer is the EnumComparison".x(() =>
            result.Comparisons[1].ShouldBeAssignableTo<EnumComparison>()
        );

        "And the 3rd comparer is the DefaultComparison".x(() =>
            result.Comparisons[2].ShouldBeAssignableTo<DefaultComparison>()
        );

        "And the 4th comparer is the DictionaryComparison".x(() =>
            result.Comparisons[3].ShouldBeAssignableTo<DictionaryComparison>()
        );

        "... with a DefaultComparison as the key comparer".x(() =>
            ((DictionaryComparison)result.Comparisons[3]).KeyComparer.ShouldBeAssignableTo<DefaultComparison>()
        );

        "... and the value comparer is the result".x(() =>
            ((DictionaryComparison)result.Comparisons[3]).ValueComparer.ShouldBeSameAs(root)
        );

        "And the 5th comparer is the DictionaryComparison".x(() =>
            result.Comparisons[4].ShouldBeAssignableTo<SetComparison>()
        );

        "... and the inner comparer is the result".x(() =>
            ((SetComparison)result.Comparisons[4]).Inner.ShouldBeSameAs(root)
        );

        "And the 6th comparer is the DictionaryComparison".x(() =>
            result.Comparisons[5].ShouldBeAssignableTo<ListComparison>()
        );

        "... and the inner comparer is the result".x(() =>
            ((ListComparison)result.Comparisons[5]).Inner.ShouldBeSameAs(root)
        );

        "And the 7th comparer is the ComplexObjectComparison".x(() =>
            result.Comparisons[6].ShouldBeAssignableTo<ComplexObjectComparison>()
        );

        "... and the inner comparer is the result".x(() =>
            ((ComplexObjectComparison)result.Comparisons[6]).Inner.ShouldBeSameAs(root)
        );

        "... and IgnoreUnmatchedProperties should be false".x(() =>
            ((ComplexObjectComparison)result.Comparisons[6]).IgnoreUnmatchedProperties.ShouldBe(false)
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
            result = ((CycleGuard)SUT.Create()).Inner as CompositeComparison
        );

        "Then the 1st comparer is the custom comparison".x(() =>
            result.Comparisons[0].ShouldBeSameAs(custom)
        );
    }

    [Scenario]
    public void Creating_a_Comparison_with_custom_comparisons_lambda()
    {
        var result = default (CompositeComparison);
        var custom = default (IComparison);
        var capture = default (IComparison);

        "Given a builder".x(() =>
            SUT = new ComparisonBuilder()
        );

        "And a custom comparison".x(() =>
        {
            custom = new Mock<IComparison>().Object;

            SUT.WithCustomComparison(x => { capture = x; return custom; });
        });

        "When calling Create".x(() =>
            result = ((CycleGuard)SUT.Create()).Inner as CompositeComparison
        );

        "Then the 1st comparer is the custom comparison".x(() =>
            result.Comparisons[0].ShouldBeSameAs(custom)
        );

        "And the lambda is given a reference to the root comparer".x(() =>
            capture.ShouldBe(result)
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
            result = ((CycleGuard)SUT.Create()).Inner as CompositeComparison
        );

        "Then the 6th comparer is the ComplexObjectComparison".x(() =>
            result.Comparisons[6].ShouldBeAssignableTo<ComplexObjectComparison>()
        );

        "... and IgnoreUnmatchedProperties should be true".x(() =>
            ((ComplexObjectComparison)result.Comparisons[6]).IgnoreUnmatchedProperties.ShouldBe(true)
        );
    }

    private class AlmostVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Revision { get; set; }
        public int Build { get; set; }
        public int MajorRevision { get; set; }
        public int MinorRevision { get; set; }
    }
}