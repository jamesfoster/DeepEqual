namespace DeepEqual.Formatting;

using System;
using System.Text;

public class DeepEqualExceptionMessageBuilder
{
    private readonly IComparisonContext context;
    private readonly IDifferenceFormatterFactory formatterFactory;

    public DeepEqualExceptionMessageBuilder(
        IComparisonContext context,
        IDifferenceFormatterFactory formatterFactory
    )
    {
        this.context = context;
        this.formatterFactory = formatterFactory;
    }

    public string GetMessage()
    {
        var sb = new StringBuilder();

        sb.Append("Comparison Failed");

        var count = context.Differences.Count;
        if (count > 0)
        {
            sb.Append($": The following {count} differences were found.");
        }

        foreach (var difference in context.Differences)
        {
            sb.Append("\n\t");

            var text = IndentLines(FormatDifference(difference));
            sb.Append(text);
        }

        return sb.ToString();
    }

    private static string IndentLines(string differenceString)
    {
        var lines = differenceString
            .Replace(Environment.NewLine, "\n")
#if NETSTANDARD2_0
            .Split(new[] { '\n' }, StringSplitOptions.None);
#else
            .Split('\n', StringSplitOptions.None);
#endif
        return string.Join("\n\t\t", lines);
    }

    private string FormatDifference(Difference difference)
    {
        var formatter = formatterFactory.GetFormatter(difference);

        return formatter.Format(difference);
    }
}
