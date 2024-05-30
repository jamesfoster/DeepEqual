namespace DeepEqual.Formatting;

using System.Text;

public class SetDifferenceFormatter : IDifferenceFormatter
{
    public string Format(Difference difference)
    {
        var setDifference = (SetDifference)difference;

        var sb = new StringBuilder();

        sb.AppendFormat(
            "{0} != {1}",
            setDifference.Breadcrumb.Left,
            setDifference.Breadcrumb.Right
        );

        if (setDifference.MissingInRight.Count != 0)
        {
            sb.AppendLine();
            sb.AppendFormat(
                "{0} contains the following unmatched elements:",
                setDifference.Breadcrumb.Left
            );
            foreach (var o in setDifference.MissingInRight)
            {
                sb.AppendLine();
                sb.AppendFormat("\t{0}", FormatterHelper.Prettify(o));
            }
        }

        if (setDifference.MissingInLeft.Count != 0)
        {
            sb.AppendLine();
            sb.AppendFormat(
                "{0} contains the following unmatched elements:",
                setDifference.Breadcrumb.Right
            );
            foreach (var o in setDifference.MissingInLeft)
            {
                sb.AppendLine();
                sb.AppendFormat("\t{0}", FormatterHelper.Prettify(o));
            }
        }

        return sb.ToString();
    }
}
