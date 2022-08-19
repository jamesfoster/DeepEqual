namespace DeepEqual.Formatting;

public class DifferenceFormatterFactory : IDifferenceFormatterFactory
{
	private readonly IDictionary<Type, IDifferenceFormatter> customFormatters;

	public DifferenceFormatterFactory(IDictionary<Type, IDifferenceFormatter> customFormatters)
	{
		this.customFormatters = customFormatters ?? new Dictionary<Type, IDifferenceFormatter>();
	}

	public IDifferenceFormatter GetFormatter(Difference difference)
	{
		if (customFormatters.TryGetValue(difference.GetType(), out var formatter))
			return formatter;

		return difference switch
		{
			MissingEntryDifference _ => new MissingEntryDifferenceFormatter(),
			SetDifference _ => new SetDifferenceFormatter(),
			BasicDifference _ => new BasicDifferenceFormatter(),
			_ => new BreadcrumbDifferenceFormatter(),
		};
	}
}