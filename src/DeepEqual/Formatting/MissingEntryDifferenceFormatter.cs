namespace DeepEqual.Formatting;

public class MissingEntryDifferenceFormatter : IDifferenceFormatter
{
    public string Format(Difference diff)
    {
        var difference = (MissingEntryDifference)diff;

        var format =
            difference.Side == MissingSide.Right
                ? "{1}[{2}] not found ({0}[{2}] = {3})"
                : "{0}[{2}] not found ({1}[{2}] = {3})";

        return string.Format(
            format,
            difference.Breadcrumb.Left,
            difference.Breadcrumb.Right,
            FormatterHelper.Prettify(difference.Key),
            FormatterHelper.Prettify(difference.Value)
        );
    }
}
