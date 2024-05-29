namespace DeepEqual.Formatting;

public class BreadcrumbDifferenceFormatter : DifferenceFormatterBase
{
    public override string Format(Difference difference)
    {
        return $"{difference.Breadcrumb.Left} != {difference.Breadcrumb.Right}";
    }
}
