namespace DeepEqual.Formatting;

using System;
using System.Linq;

public class BasicDifferenceFormatter : DifferenceFormatterBase
{
    private const int InitialMaxLength = 20;
    private const int HalfMaxLength = InitialMaxLength / 2;

    public override string Format(Difference difference)
    {
        var diff =
            difference as BasicDifference
            ?? throw new ArgumentException("Invalid difference type", nameof(difference));

        var value1 = diff.Value1;
        var value2 = diff.Value2;

        if (value1 is string str1 && value2 is string str2)
            (value1, value2) = FixLongStringDifference(str1, str2);

        var breadcrumb = diff.Breadcrumb.Dot(diff.LeftChildProperty, diff.RightChildProperty);

        return string.Format(
            "Actual{0} != Expected{1} ({2} != {3})",
            breadcrumb.Left,
            breadcrumb.Right,
            Prettify(value1),
            Prettify(value2)
        );
    }

    private static (string, string) FixLongStringDifference(string value1, string value2)
    {
        var maxLength1 = InitialMaxLength;
        var maxLength2 = InitialMaxLength;

        var firstDiffIndex = FindIndexOfFirstDifferentChar(value1, value2);

        var lowerBound = Math.Max(0, firstDiffIndex - HalfMaxLength);

        if (lowerBound >= 3)
        {
            if (value1.Length > maxLength1)
            {
                value1 = "..." + value1.Substring(Math.Min(lowerBound, value1.Length - maxLength1));
                maxLength1 += 3;
            }

            if (value2.Length > maxLength2)
            {
                value2 = "..." + value2.Substring(Math.Min(lowerBound, value2.Length - maxLength2));
                maxLength2 += 3;
            }
        }

        if (value1.Length > maxLength1 + 3)
            value1 = value1.Substring(0, maxLength1) + "...";
        if (value2.Length > maxLength2 + 3)
            value2 = value2.Substring(0, maxLength2) + "...";

        return (value1, value2);
    }

    private static int FindIndexOfFirstDifferentChar(string value1, string value2)
    {
        return value1.Zip(value2, (a, b) => a == b).TakeWhile(x => x).Count();
    }
}
