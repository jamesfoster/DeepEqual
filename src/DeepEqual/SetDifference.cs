namespace DeepEqual;

public class SetDifference : Difference
{
	public ImmutableList<object> Expected { get; }
	public ImmutableList<object> Extra { get; }

	public SetDifference(string breadcrumb, IEnumerable<object> expected, IEnumerable<object> extra) : base(breadcrumb)
	{
		Expected = expected.ToImmutableList();
		Extra = extra.ToImmutableList();
	}
}