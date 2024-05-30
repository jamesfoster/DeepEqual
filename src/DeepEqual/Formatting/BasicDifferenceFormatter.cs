namespace DeepEqual.Formatting;

using System;
using System.Linq;

public class BasicDifferenceFormatter : IDifferenceFormatter
{
    private const int InitialMaxLength = 20;
    private const int HalfMaxLength = InitialMaxLength / 2;

    public string Format(Difference difference)
    {
        var diff =
            difference as BasicDifference
            ?? throw new ArgumentException("Invalid difference type", nameof(difference));

        var leftValue = diff.LeftValue;
        var rightValue = diff.RightValue;

        if (leftValue is string str1 && rightValue is string str2)
            (leftValue, rightValue) = FixLongStringDifference(str1, str2);

        var breadcrumb = diff.Breadcrumb.Dot(diff.LeftChildProperty, diff.RightChildProperty);

        return string.Format(
            "{0} != {1} ({2} != {3})",
            breadcrumb.Left,
            breadcrumb.Right,
            FormatterHelper.Prettify(leftValue),
            FormatterHelper.Prettify(rightValue)
        );
    }

    private static (string, string) FixLongStringDifference(string leftValue, string rightValue)
    {
        var maxLength1 = InitialMaxLength;
        var maxLength2 = InitialMaxLength;

        var firstDiffIndex = FindIndexOfFirstDifferentChar(leftValue, rightValue);

        var lowerBound = Math.Max(0, firstDiffIndex - HalfMaxLength);

        if (lowerBound >= 3)
        {
            if (leftValue.Length > maxLength1)
            {
                leftValue =
                    "..."
                    + leftValue.Substring(Math.Min(lowerBound, leftValue.Length - maxLength1));
                maxLength1 += 3;
            }

            if (rightValue.Length > maxLength2)
            {
                rightValue =
                    "..."
                    + rightValue.Substring(Math.Min(lowerBound, rightValue.Length - maxLength2));
                maxLength2 += 3;
            }
        }

        if (leftValue.Length > maxLength1 + 3)
            leftValue = leftValue.Substring(0, maxLength1) + "...";
        if (rightValue.Length > maxLength2 + 3)
            rightValue = rightValue.Substring(0, maxLength2) + "...";

        return (leftValue, rightValue);
    }

    private static int FindIndexOfFirstDifferentChar(string leftValue, string rightValue)
    {
        return leftValue.Zip(rightValue, (a, b) => a == b).TakeWhile(x => x).Count();
    }
}
