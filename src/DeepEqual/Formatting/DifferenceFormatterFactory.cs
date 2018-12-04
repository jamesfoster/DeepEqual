namespace DeepEqual.Formatting
{
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

				default:
					return new BasicDifferenceFormatter();
			}
		}
	}
}