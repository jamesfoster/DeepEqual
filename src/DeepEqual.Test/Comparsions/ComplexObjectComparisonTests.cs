using AutoFixture;

using DeepEqual.Test.Helper;

using Shouldly;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using Xbehave;

using Xunit;

namespace DeepEqual.Test.Comparsions;

public class ComplexObjectComparisonTests
{
    private readonly Fixture fixture = new();

    private ComplexObjectComparison SUT;
    private MockComparison inner;
    private IComparisonContext context;

    private ComparisonResult result;

    private readonly List<Func<PropertyPair, bool>> ignoredProperties = [];
    private readonly List<Func<Type, Type, string, string>> mappedProperties = [];

    [Scenario]
    public void Creating_a_ComplexObjectComparer()
    {
        "When creating a ComplexObjectComparison".x(() =>
            SUT = new ComplexObjectComparison(
                ignoreUnmatchedProperties: false,
                ignoredProperties: [],
                mappedProperties: []
            )
        );

        "It should be an IComparison".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    private void SetUp(bool ignoreUnmatchedProperties = false)
    {
        "Given an inner comparison".x(() =>
        {
            inner = new MockComparison();
        });

        "And a ComplexObjectComparison".x(() =>
            SUT = new ComplexObjectComparison(ignoreUnmatchedProperties, ignoredProperties, mappedProperties)
        );

        "And a Comparison context object".x(() =>
            context = new ComparisonContext(inner, new BreadcrumbPair("Property"))
        );
    }

    [Scenario]
    [MemberData(nameof(SimilarObjectsTestData))]
    public void When_comparing_objects_of_the_same_type(
        bool ignoreUnmatchedProperties,
        object leftValue,
        object rightValue,
        ComparisonResult expected
    )
    {
        SetUp(ignoreUnmatchedProperties);

        "When calling Compare".x(() =>
        {
            (result, context) = SUT.Compare(context, leftValue, rightValue);
        });

        "It should return {3}".x(() =>
            result.ShouldBe(expected)
        );

        if (expected == ComparisonResult.Fail)
        {
            "And it should add a difference to the context".x(() =>
                context.Differences.Count.ShouldBe(1)
            );
        }
        else
        {
            "And there should be no differences in the context".x(() =>
                context.Differences.Count.ShouldBe(0)
            );
        }

        "And it should call Compare on the inner comparison for each property".x(() =>
        {
            var properties1 = leftValue.GetType().GetProperties();
            var properties2 = rightValue.GetType().GetProperties().ToDictionary(x => x.Name);

            foreach (var p1 in properties1)
            {
                if (!properties2.ContainsKey(p1.Name))
                    continue;

                var p2 = properties2[p1.Name];

                var v1 = p1.GetValue(leftValue);
                var v2 = p2.GetValue(rightValue);

                inner.CompareCalls.ShouldContain(call =>
                    call.context.Breadcrumb.Left == "Property." + p1.Name &&
                    call.context.Breadcrumb.Right == "Property." + p1.Name &&
                    call.leftValue.Equals(v1) &&
                    call.rightValue.Equals(v2)
                );
            }
        });
    }

    [Scenario]
    public void Ignoring_properties_on_the_left_type_when_missing()
    {
        A leftValue = null;
        object rightValue = null;

        SetUp();

        "And the IgnoreMe property is ignored".x(() =>
            ignoredProperties.Add((property) => property.LeftName == "IgnoreMe")
        );

        "And leftValue is provided".x(() =>
            leftValue = new A
            {
                X = fixture.Create<string>(),
                Y = fixture.Create<string>(),
                IgnoreMe = fixture.Create<string>()
            }
        );

        "And rightValue is equivalent to leftValue".x(() =>
            rightValue = new
            {
                leftValue.X,
                leftValue.Y
            }
        );

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Ignoring_properties_on_the_left_type_when_different()
    {
        A leftValue = null;
        object rightValue = null;

        SetUp();

        "And the IgnoreMe property is ignored".x(() =>
            ignoredProperties.Add((property) => property.LeftName == "IgnoreMe")
        );

        "And leftValue is a MyClass instance".x(() =>
            leftValue = new A
            {
                X = fixture.Create<string>(),
                Y = fixture.Create<string>(),
                IgnoreMe = fixture.Create<string>()
            }
        );

        "And rightValue is equivalent to leftValue".x(() =>
            rightValue = new
            {
                leftValue.X,
                leftValue.Y,
                IgnoreMe = fixture.Create<string>()
            }
        );

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Ignoring_properties_on_the_right_type_when_missing()
    {
        object leftValue = null;
        A rightValue = null;

        SetUp();

        "And the IgnoreMe property is ignored".x(() =>
            ignoredProperties.Add((property) => property.LeftName == "IgnoreMe")
        );

        "And rightValue".x(() =>
            rightValue = new A
            {
                X = fixture.Create<string>(),
                Y = fixture.Create<string>(),
                IgnoreMe = fixture.Create<string>()
            });

        "And leftValue is equivalent to rightValue".x(() =>
            leftValue = new
            {
                rightValue.X,
                rightValue.Y
            }
        );

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Ignoring_properties_on_the_right_type_when_different()
    {
        object leftValue = null;
        A rightValue = null;

        SetUp();

        "And the IgnoreMe property is ignored".x(() =>
            ignoredProperties.Add((property) => property.LeftName == "IgnoreMe")
        );

        "And rightValue".x(() =>
            rightValue = new A
            {
                X = fixture.Create<string>(),
                Y = fixture.Create<string>(),
                IgnoreMe = fixture.Create<string>()
            }
        );

        "And leftValue is equivalent to rightValue".x(() =>
            leftValue = new
                {
                    rightValue.X,
                    rightValue.Y,
                    IgnoreMe = fixture.Create<string>()
                }
        );

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Ignoring_properties_on_a_base_class_also_ignores_them_on_derived_types()
    {
        B leftValue = null;
        object rightValue = null;

        SetUp();

        "And the IgnoreMe property is ignored".x(() =>
            ignoredProperties.Add((property) => property.LeftName == "IgnoreMe")
        );

        "And leftValue".x(() =>
            leftValue = new B
            {
                X = fixture.Create<string>(),
                Y = fixture.Create<string>(),
                IgnoreMe = fixture.Create<string>()
            }
        );

        "And rightValue is equivalent to leftValue".x(() =>
            rightValue = new
                {
                    leftValue.X,
                    leftValue.Y,
                    IgnoreMe = fixture.Create<string>()
                }
        );

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Comparing_object_with_new_property_uses_value_in_derived_type()
    {
        Derived leftValue = null;
        object rightValue = null;

        SetUp();

        "And two values".x(() =>
        {
            leftValue = new Derived();
            rightValue = new { Property = "abc" };
        });

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Properties_with_no_getter_are_ignored()
    {
        C leftValue = null;
        object rightValue = null;

        SetUp();

        "And leftValue is provided".x(() =>
            leftValue = new C
            {
                X = fixture.Create<string>(),
                SetOnly = fixture.Create<string>()
            }
        );

        "And rightValue is equivalent to leftValue".x(() =>
            rightValue = new
            {
                leftValue.X,
                leftValue.Y
            }
        );

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void Comparing_dynamic_object_with_static_object_succeeds()
    {
        dynamic leftValue = null;
        object rightValue = null;

        SetUp();

        "And a dynamic object".x(() =>
        {
            leftValue = new ExpandoObject();
            leftValue.Foo = "abc";
            leftValue.Bar = 123;
        });

        "And a static object".x(() =>
        {
            rightValue = new
            {
                Foo = "abc",
                Bar = 123
            };
        });

        "When comparing the 2 values".x(() =>
            (result, _) = SUT.Compare(context, rightValue, rightValue)
        );

        "Then it should return a Pass".x(() =>
            result.ShouldBe(ComparisonResult.Pass)
        );
    }

    private class A
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string IgnoreMe { get; set; }
    }

    private class B : A { }

    private class C
    {
        public string X { get; set; }

        public string Y { get; set; }

        public string SetOnly
        {
            set => Y = value;
        }
    }

    public interface IFoo
    {
        int Prop { get; set; }
    }

    public class Base
    {
        public int Property => 123;
    }

    public class Derived : Base
    {
        public new string Property => "abc";
    }

    public static IEnumerable<object[]> SimilarObjectsTestData => [
        [
            false,
            new {A = 1, B = 2, C = 3},
            new {A = 1, B = 2, C = 3},
            ComparisonResult.Pass
        ],
        [
            false,
            new {},
            new {},
            ComparisonResult.Pass
        ],
        [
            false,
            new {A = 1, B = 2, C = 3},
            new {A = 1, B = 2},
            ComparisonResult.Fail
        ],
        [
            false,
            new {A = 1, B = 2},
            new {A = 1, B = 2, C = 3},
            ComparisonResult.Fail
        ],
        [
            false,
            new {A = 1, B = 2, C = 3},
            new {A = 123, B = 2, C = 3},
            ComparisonResult.Fail
        ],
        [
            true,
            new {A = 1, B = 2, C = 3},
            new {A = 1, B = 2, C = 3},
            ComparisonResult.Pass
        ],
        [
            true,
            new {A = 1, B = 2, C = 3},
            new {A = 1, B = 2},
            ComparisonResult.Pass
        ],
        [
            true,
            new {A = 1, B = 2},
            new {A = 1, B = 2, C = 3},
            ComparisonResult.Pass
        ],
        [
            true,
            new {A = 1, B = 2, C = 3},
            new {A = 123, B = 2, C = 3},
            ComparisonResult.Fail
        ]
    ];
}