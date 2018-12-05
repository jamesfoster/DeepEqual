namespace DeepEqual.Formatting
{
	using System;

	public class DifferenceFormatterFactory
	{
		public IDifferenceFormatter GetFormatter(Difference difference)
		{
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