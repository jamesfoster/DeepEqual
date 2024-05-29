using DeepEqual.Syntax;

using Shouldly;

using Xunit;

namespace DeepEqual.Test.Helper;

public static class DeepAssert
{
    public static void AreEqual(object actual, object expected, IComparison comparison = null)
    {
        actual.IsDeepEqual(expected, comparison).ShouldBe(true);
        actual.ShouldDeepEqual(expected, comparison);
    }

    public static void AreNotEqual(object actual, object expected, IComparison comparison = null)
    {
        actual.IsDeepEqual(expected, comparison).ShouldBe(false);
        Assert.Throws<DeepEqualException>(() => actual.ShouldDeepEqual(expected, comparison));
    }
}