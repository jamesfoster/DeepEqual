namespace DeepEqual.Formatting
{
	using System;
	using System.Collections.Generic;

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

			switch (difference)
			{
				case MissingEntryDifference _:
					return new MissingEntryDifferenceFormatter();

				case SetDifference _:
					return new SetDifferenceFormatter();

				case BasicDifference _:
					return new BasicDifferenceFormatter();

				default:
					return new BreadcrumbDifferenceFormatter();
			}
		}
	}
}