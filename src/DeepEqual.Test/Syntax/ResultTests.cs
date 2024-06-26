﻿using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using Shouldly;

using Xunit;

namespace DeepEqual.Test.Syntax;

public class ResultTests
{
    private object a = new object();
    private object b = new object();

    [Fact]
    public void PassResult()
    {
        var comparison = new EchoComparison(ComparisonResult.Pass);

        DeepAssert.AreEqual(a, b, comparison);
    }

    [Fact]
    public void FailResult()
    {
        var comparison = new EchoComparison(ComparisonResult.Fail);

        DeepAssert.AreNotEqual(a, b, comparison);
    }

    [Fact]
    public void InconclusiveResult()
    {
        var comparison = new EchoComparison(ComparisonResult.Inconclusive);

        DeepAssert.AreNotEqual(a, b, comparison);
    }

    [Fact]
    public void ExceptionContainsExpectedInfo()
    {
        var value1 = new { A = 1 };
        var value2 = new { A = 2 };

        var exception = Assert.Throws<DeepEqualException>(() => value1.ShouldDeepEqual(value2));

        var difference = exception.Context.Differences
            .ShouldHaveSingleItem()
            .ShouldBeAssignableTo<BasicDifference>();

        difference.Breadcrumb.Left.ShouldBe("Left.A");
        difference.Breadcrumb.Right.ShouldBe("Right.A");
        difference.LeftValue.ShouldBe(1);
        difference.RightValue.ShouldBe(2);
    }
}