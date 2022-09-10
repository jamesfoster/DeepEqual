namespace DeepEqual.Formatting;

public class BreadcrumbDifferenceFormatter : DifferenceFormatterBase
{
	public override string Format(Difference difference)
	{
		return $"Actual{difference.Breadcrumb} != Expected{difference.Breadcrumb}";
	}
}