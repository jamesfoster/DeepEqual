using System;
using System.Collections.Generic;

using DeepEqual.Formatting;

using Xunit;

namespace DeepEqual.Test;

public class ExceptionMessageTests
{
    [Fact]
    public void No_differences_recorded()
    {
        var context = new ComparisonContext(rootComparison: null!);

        AssertExceptionMessage(
            context,
            expectedMessage: "Comparison Failed"
        );
    }

    [Fact]
    public void Null_difference()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(null, new object());

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ((null) != System.Object)
""");
    }

    [Fact]
    public void Single_string_difference()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference("a", "b");

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ("a" != "b")
""");
    }

    [Fact]
    public void Single_int_difference()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(1, 2);

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right (1 != 2)
""");
    }

    [Fact]
    public void List_int_difference()
    {
        var root = new ComparisonContext(rootComparison: null!);
        var childContext1 = root
            .VisitingIndex(2)
            .AddDifference(1, 2);
        var childContext2 = root
            .VisitingIndex(4)
            .AddDifference(4, 5);
        var context = root
            .MergeDifferencesFrom(childContext1)
            .MergeDifferencesFrom(childContext2);

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 2 differences were found.
{'\t'}Left[2] != Right[2] (1 != 2)
{'\t'}Left[4] != Right[4] (4 != 5)
""");
    }

    [Fact]
    public void Long_string_difference_start()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(
                "01234567890123456789012345678901234567890123456789",
                "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij"
            );

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ("01234567890123456789..." != "abcdefghijabcdefghij...")
""");
    }

    [Fact]
    public void Long_string_difference_middle()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(
                "01234567890123456789012345first678901234567890123456789",
                "01234567890123456789012345second678901234567890123456789"
            );

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ("...6789012345first67890..." != "...6789012345second6789...")
""");
    }

    [Fact]
    public void Long_string_difference_end()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(
                "01234567890123456789012345678901234567890123456789a",
                "01234567890123456789012345678901234567890123456789b"
            );

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ("...1234567890123456789a" != "...1234567890123456789b")
""");
    }

    [Fact]
    public void Long_string_difference_end_2()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(
                "01234567890123456789012345678901234567890123",
                "01234567890123456789012345678901234567890123456789"
            );

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ("...45678901234567890123" != "...01234567890123456789")
""");
    }

    [Theory]
    [InlineData("0123456789012345678", "01234567890123456789", "0123456789012345678", "01234567890123456789")]
    [InlineData("012345678901234567", "0123456789012345678", "012345678901234567", "0123456789012345678")]
    [InlineData("0123456789012345678", "012345678901234567890", "0123456789012345678", "...12345678901234567890")]
    [InlineData("012345678901234567890", "0123456789012345678", "...12345678901234567890", "0123456789012345678")]
    public void Strings_around_same_length_as_max_length(string value1, string value2, string expected1, string expected2)
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(
                value1,
                value2
            );

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left != Right ("{expected1}" != "{expected2}")
""");
    }

    [Fact]
    public void Missing_expected_entry()
    {
        IComparisonContext context = new ComparisonContext(rootComparison: null!);
        context = context
            .AddDifference(new MissingEntryDifference(
                Breadcrumb: context.Breadcrumb,
                Side: MissingSide.Right,
                Key: "Index",
                Value: "Value"
            ));

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Right["Index"] not found (Left["Index"] = "Value")
""");
    }

    [Fact]
    public void Missing_actual_entry()
    {
        IComparisonContext context = new ComparisonContext(rootComparison: null!);
        context = context
            .AddDifference(new MissingEntryDifference(
                Breadcrumb: context.Breadcrumb,
                Side: MissingSide.Left,
                Key: "Index",
                Value: "Value"
            ));

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left["Index"] not found (Right["Index"] = "Value")
""");
    }

    [Fact]
    public void Set_difference_expected()
    {
        IComparisonContext context = new ComparisonContext(rootComparison: null!);
        context = context
            .AddDifference(new SetDifference(
                breadcrumb: context.Breadcrumb.Dot("Set"),
                missingInLeft: new List<object> { 1, 2, 3 },
                missingInRight: new List<object>()
            ));

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{"\t"}Left.Set != Right.Set
{"\t\t"}Right.Set contains the following unmatched elements:
{"\t\t\t"}1
{"\t\t\t"}2
{"\t\t\t"}3
""");
    }

    [Fact]
    public void Set_difference_actual()
    {
        IComparisonContext context = new ComparisonContext(rootComparison: null!);
        context = context
            .AddDifference(new SetDifference(
                breadcrumb: context.Breadcrumb.Dot("Set"),
                missingInLeft: new List<object>(),
                missingInRight: new List<object> {1, 2, 3}
            ));

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{"\t"}Left.Set != Right.Set
{"\t\t"}Left.Set contains the following unmatched elements:
{"\t\t\t"}1
{"\t\t\t"}2
{"\t\t\t"}3
""");
    }

    [Fact]
    public void Custom_difference_type()
    {
        IComparisonContext context = new ComparisonContext(rootComparison: null!);
        context = context
            .AddDifference(new CustomDifference(context.Breadcrumb.Dot("Custom"), 123));

        AssertExceptionMessage(
            context,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}Left.Custom != Right.Custom
""");
    }

    [Fact]
    public void Custom_difference_type_formatter()
    {
        var context = new ComparisonContext(rootComparison: null!)
            .AddDifference(new CustomDifference(new BreadcrumbPair(".Custom"), 123));

        var customFormatters = new Dictionary<Type, IDifferenceFormatter>
        {
            { typeof(CustomDifference), new CustomDifferenceFormatter() }
        };

        AssertExceptionMessage(
            context,
            customFormatters: customFormatters,
            expectedMessage: $"""
Comparison Failed: The following 1 differences were found.
{'\t'}.Custom >>>123<<<
""");
    }

    public record CustomDifference(BreadcrumbPair Breadcrumb, int Foo) : Difference(Breadcrumb);

    public class CustomDifferenceFormatter : IDifferenceFormatter
    {
        public string Format(Difference difference)
        {
            var customDifference = (CustomDifference) difference;

            return $"{difference.Breadcrumb.Left} >>>{customDifference.Foo}<<<";
        }
    }

    private static void AssertExceptionMessage(
        IComparisonContext context,
        string expectedMessage,
        Dictionary<Type, IDifferenceFormatter> customFormatters = null)
    {
        expectedMessage = expectedMessage.Trim().Replace("\r\n", "\n");

        var messageBuilder = new DeepEqualExceptionMessageBuilder(
            context,
            new DifferenceFormatterFactory(customFormatters)
        );

        var message = messageBuilder.GetMessage().Replace("\r\n", "\n");

        Assert.Equal(expectedMessage, message);
    }
}