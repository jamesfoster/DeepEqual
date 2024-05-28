namespace DeepEqual.Formatting;

public class MissingEntryDifferenceFormatter : DifferenceFormatterBase
{
    public override string Format(Difference diff)
    {
        var difference = (MissingEntryDifference)diff;

        var format =
            difference.Side == MissingSide.Expected
                ? "Expected{1}[{2}] not found (Actual{0}[{2}] = {3})"
                : "Actual{0}[{2}] not found (Expected{1}[{2}] = {3})";

        return string.Format(
            format,
            difference.Breadcrumb.Left,
            difference.Breadcrumb.Right,
            Prettify(difference.Key),
            Prettify(difference.Value)
        );
    }
}
