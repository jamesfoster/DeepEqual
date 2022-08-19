namespace DeepEqual;

public class MissingEntryDifference : Difference
{
	public MissingSide Side { get; }
	public object Key { get; }
	public object Value { get; }

	public MissingEntryDifference(string breadcrumb, MissingSide side, object key, object value) : base(breadcrumb)
	{
		Side = side;
		Key = key;
		Value = value;
	}
}

public enum MissingSide
{
	Actual,
	Expected
}