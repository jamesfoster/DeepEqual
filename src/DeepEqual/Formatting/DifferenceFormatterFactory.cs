namespace DeepEqual.Formatting
{
	public class DifferenceFormatterFactory
	{
		public IDifferenceFormatter GetFormatter(Difference difference)
		{
			if (difference is MissingEntryDifference)
			{
				return new MissingEntryDifferenceFormatter();
			}

			if (difference is SetDifference)
			{
				return new SetDifferenceFormatter();
			}

			return new BasicDifferenceFormatter();
		}
	}
}