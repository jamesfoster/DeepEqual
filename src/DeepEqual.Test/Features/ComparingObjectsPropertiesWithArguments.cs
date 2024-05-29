
using DeepEqual.Test.Helper;
using DeepEqual.Test.VB;

using Xunit;

namespace DeepEqual.Test.Features;

public class ComparingObjectsPropertiesWithArguments
{
    [Fact]
    public void ShouldBeAbleToHandleComparingObjectsWithPropertiesWithArguments()
    {
        var x = new HasPropertiesWithArguments();
        var y = new HasPropertiesWithArguments();

        DeepAssert.AreEqual(x, y);
    }
}
