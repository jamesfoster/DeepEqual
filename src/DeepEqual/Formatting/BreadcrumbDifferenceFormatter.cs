namespace DeepEqual.Formatting;

public class BreadcrumbDifferenceFormatter : DifferenceFormatterBase
{
    public override string Format(Difference difference)
    {
        return $"Actual{difference.Breadcrumb.Left} != Expected{difference.Breadcrumb.Right}";
    }
}
