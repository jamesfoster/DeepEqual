using DeepEqual.Syntax;
using DeepEqual.Test.Helper;

using Shouldly;

using Xunit;

namespace DeepEqual.Test.Features;

public class ObjectsWithCyclicalReferences
{
    private readonly Data expected;
    private readonly Data actual;

    public ObjectsWithCyclicalReferences()
    {
        expected = new Data
        {
            Name = "Joe",
            Parent = new Data { Name = "Jack" }
        };
        expected.Parent.Child = expected;

        actual = new Data
        {
            Name = "Joe",
            Parent = new Data { Name = "Jack" }
        };
        actual.Parent.Child = actual;

    }

    [Fact]
    public void Default_behaviour_is_to_throw_on_detected_cycle()
    {
        Assert.Throws<ObjectGraphCircularReferenceException>(
            () => DeepAssert.AreEqual(actual, expected)
        );
    }


    [Fact]
    public void Can_change_behaviour_to_ignore_detected_cycles()
    {
        var builder = actual
            .WithDeepEqual(expected)
            .IgnoreCircularReferences();

        builder.Assert();
        builder.Compare().ShouldBe(true);
    }

    public class Data
    {
        public string Name { get; set; }
        public Data Parent { get; set; }
        public Data Child { get; set; }
    }
}