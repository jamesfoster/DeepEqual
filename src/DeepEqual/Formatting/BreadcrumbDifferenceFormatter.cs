namespace DeepEqual.Formatting;

public class BreadcrumbDifferenceFormatter : IDifferenceFormatter
{
    public string Format(Difference difference)
    {
        return $"{difference.Breadcrumb.Left} != {difference.Breadcrumb.Right}";
    }
}
