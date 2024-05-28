namespace DeepEqual.Formatting;

using System.Text;

public class SetDifferenceFormatter : DifferenceFormatterBase
{
    public override string Format(Difference difference)
    {
        var setDifference = (SetDifference)difference;

        var sb = new StringBuilder();

        sb.AppendFormat(
            "Actual{0} != Expected{1}",
            setDifference.Breadcrumb.Left,
            setDifference.Breadcrumb.Right
        );

        if (setDifference.Extra.Count != 0)
        {
            sb.AppendLine();
            sb.AppendFormat(
                "Actual{0} contains the following unmatched elements:",
                setDifference.Breadcrumb.Left
            );
            foreach (var o in setDifference.Extra)
            {
                sb.AppendLine();
                sb.AppendFormat("\t{0}", Prettify(o));
            }
        }

        if (setDifference.Expected.Count != 0)
        {
            sb.AppendLine();
            sb.AppendFormat(
                "Expected{0} contains the following unmatched elements:",
                setDifference.Breadcrumb.Right
            );
            foreach (var o in setDifference.Expected)
            {
                sb.AppendLine();
                sb.AppendFormat("\t{0}", Prettify(o));
            }
        }

        return sb.ToString();
    }
}
