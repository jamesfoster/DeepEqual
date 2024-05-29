using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using Shouldly;

using System;

using Xunit;

namespace DeepEqual.Test.Features;

public class MappingPropertyNames
{
    [Fact]
    public static void Should_consider_mapped_properties_equivalent()
    {
        var left = new Left(Id: 1);
        var right = new Right(Key: 1);

        left.WithDeepEqual(right).MapProperty<Left, Right>(x => x.Id, x => x.Key).Assert();
    }


    [Fact]
    public static void Should_consider_mapped_properties_different()
    {
        var left = new Left(Id: 1);
        var right = new Right(Key: 2);

        var action = () =>
            left
                .WithDeepEqual(right)
                .MapProperty<Left, Right>(x => x.Id, x => x.Key)
                .Assert();

        var exception = Assert.Throws<DeepEqualException>(action);

        var difference = exception.Context.Differences
            .ShouldHaveSingleItem()
            .ShouldBeAssignableTo<BasicDifference>();

        difference.Breadcrumb.Left.ShouldBe("Left.Id");
        difference.Breadcrumb.Right.ShouldBe("Right.Key");
        difference.LeftValue.ShouldBe(1);
        difference.RightValue.ShouldBe(2);

    }

    public record Left(int Id);
    public record Right(int Key);
}