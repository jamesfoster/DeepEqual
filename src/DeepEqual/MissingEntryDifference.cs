namespace DeepEqual
{
	public class MissingEntryDifference : Difference
	{
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