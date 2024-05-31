using DeepEqual.Test.Helper;

using Moq;

using Shouldly;

using System;
using System.Collections.Generic;
using System.Linq;

using Xbehave;

namespace DeepEqual.Test.Comparsions;

public class CompositComparisonTests
{
    private CompositeComparison SUT { get; set; }
    private List<Mock<IComparison>> Inner { get; set; }
    private IComparisonContext Context { get; set; }

    private ComparisonResult Result { get; set; }

    [Background]
    public void SetUp()
    {
        "Given some inner comparers".x(() =>
        {
            Inner = [new Mock<IComparison>(), new Mock<IComparison>(), new Mock<IComparison>()];

            Inner.ForEach(
                m => m
                    .Setup(c => c.CanCompare(It.IsAny<IComparisonContext>(), It.IsAny<Type>(), It.IsAny<Type>()))
                    .Returns(false)
            );
        });

        "... which by default return Inconclusive".x(() =>
        {
            Inner.ForEach(
                m => m
                    .Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
                    .Returns<IComparisonContext, object, object>((c, v1, v2) => (ComparisonResult.Inconclusive, c))
            );
        });

        "And a CompositeComparer".x(() =>
            SUT = new CompositeComparison(Inner.Select(x => x.Object))
        );
    }

    [Scenario]
    public void When_creating_a_CompositeComparer()
    {
        "When creating a CompostieComperer".x(() =>
            SUT = new CompositeComparison()
        );

        "it should implement IComparer".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );

        "CanCompare should always true".x(() =>
            SUT.CanCompare(context: null, leftType: null, rightType: null).ShouldBe(true)
        );
    }

    [Scenario]
    public void When_adding_a_caomparison()
    {
        "Given a CompostieComperer".x(() =>
            SUT = new CompositeComparison()
        );

        "When adding a comparer".x(() =>
            SUT.Add(Mock.Of<IComparison>())
        );

        "Then there should be one comparison".x(() =>
            SUT.Comparisons.Count.ShouldBe(1)
        );
    }

    [Scenario]
    public void When_testing_equality_if_a_comparer_returns_Inconclusive(object leftValue, object rightValue)
    {
        "Given the first comparer can compare the values".x(() =>
            Inner[0]
                .Setup(c => c.CanCompare(It.IsAny<IComparisonContext>(), It.IsAny<Type>(), It.IsAny<Type>()))
                .Returns(true)
        );

        "... and returns Inconclusive".x(() =>
            Inner[0]
                .Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
                .Returns<IComparisonContext, object, object>((c, v1, v2) => (ComparisonResult.Inconclusive, c))
        );

        "And some values to compare".x(() =>
        {
            leftValue = new object();
            rightValue = new object();
        });

        "And a Comparison context object".x(() =>
            Context = new ComparisonContext(rootComparison: null!)
        );

        "When calling Compare".x(() =>
            (Result, _) = SUT.Compare(Context, leftValue, rightValue)
        );

        "Then it should call CanCompare on all inner comparisons".x(() =>
            Inner.VerifyAll(c => c.CanCompare(Context, typeof(object), typeof(object)), Times.Once())
        );

        "and it should call Compare on the first inner comparison".x(() =>
            Inner[0].Verify(c => c.Compare(Context, leftValue, rightValue), Times.Once())
        );

        "but it shouldn't call the other comparisons Compare".x(() =>
            Inner.Skip(1).VerifyAll(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never())
        );

        "and it should return Inconclusive".x(() =>
            Result.ShouldBe(ComparisonResult.Inconclusive)
        );
    }

    [Scenario]
    public void When_testing_equality_if_a_comparer_returns_Pass(object leftValue, object rightValue)
    {
        "Given the first comparer can compare the values".x(() =>
            Inner[0]
                .Setup(c => c.CanCompare(It.IsAny<IComparisonContext>(), It.IsAny<Type>(), It.IsAny<Type>()))
                .Returns(true)
        );

        "... and returns Pass".x(() =>
            Inner[0]
                .Setup(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()))
                .Returns<IComparisonContext, object, object>((c, v1, v2) => (ComparisonResult.Pass, c))
        );

        "And some values to compare".x(() =>
        {
            leftValue = new object();
            rightValue = new object();
        });

        "And a Comparison context object".x(() =>
            Context = new ComparisonContext(rootComparison: null!)
        );

        "When calling Compare".x(() =>
            (Result, _) = SUT.Compare(Context, leftValue, rightValue)
        );

        "Then it should call CanCompare on the first inner comparisons".x(() =>
            Inner[0].Verify(c => c.CanCompare(Context, typeof(object), typeof(object)), Times.Once())
        );

        "and it should call Compare on the first inner comparison".x(() =>
            Inner[0].Verify(c => c.Compare(Context, leftValue, rightValue), Times.Once())
        );

        "but it shouldn't call the other comparisons CanCompare".x(() =>
            Inner.Skip(1).VerifyAll(c => c.CanCompare(It.IsAny<IComparisonContext>(), It.IsAny<Type>(), It.IsAny<Type>()), Times.Never())
        );

        "and it shouldn't call the other comparisons Compare".x(() =>
            Inner.Skip(1).VerifyAll(c => c.Compare(It.IsAny<IComparisonContext>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never())
        );

        "and it should return Pass".x(() =>
            Result.ShouldBe(ComparisonResult.Pass)
        );
    }

    [Scenario]
    public void When_testing_equality_if_all_comparers_cant_compare_the_values(object leftValue, object rightValue)
    {
        "Given some values to compare".x(() =>
        {
            leftValue = new object();
            rightValue = new object();
        });

        "And a Comparison context object".x(() =>
            Context = new ComparisonContext(rootComparison: null!)
        );

        "When calling Compare".x(() =>
            (Result, _) = SUT.Compare(Context, rightValue, rightValue)
        );

        "it should call the inner comparers CanCompare".x(() =>
            Inner.VerifyAll(c => c.CanCompare(Context, typeof(object), typeof(object)), Times.Once())
        );

        "it should not call the inner comparers Compare".x(() =>
            Inner.VerifyAll(c => c.Compare(Context, leftValue, rightValue), Times.Never())
        );

        "and it should return false".x(() =>
            Result.ShouldBe(ComparisonResult.Inconclusive)
        );
    }

    [Scenario]
    [Example(null, 1, ComparisonResult.Fail)]
    [Example(1, null, ComparisonResult.Fail)]
    [Example(null, null, ComparisonResult.Pass)]
    public void When_testing_nulls(object leftValue, object rightValue, ComparisonResult expected)
    {
        "Given a Comparison context object".x(() =>
            Context = new ComparisonContext(rootComparison: null!)
        );

        "When calling Compare".x(() =>
            (Result, _) = SUT.Compare(Context, leftValue, rightValue)
        );

        "Then it should return {2}".x(() =>
            Result.ShouldBe(expected)
        );

        "And it should not call the inner comparers CanCompare".x(() =>
            Inner.VerifyAll(c => c.CanCompare(It.IsAny<IComparisonContext>(), It.IsAny<Type>(), It.IsAny<Type>()), Times.Never())
        );

        "And it should not call the inner comparers Compare".x(() =>
            Inner.VerifyAll(c => c.Compare(Context, leftValue, rightValue), Times.Never())
        );
    }
}