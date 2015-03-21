namespace DeepEqual
{
	public class MissingEntryDifference : Difference
	{
		public MissingEntryDifference(MissingSide side, object key, object value)
		{
			Side = side;
			Key = key;
			Value = value;
		}

		public MissingSide Side { get; set; }
		public object Key { get; set; }
		public object Value { get; set; }
	}

	public enum MissingSide
	{
		Actual,
		Expected
	}

}