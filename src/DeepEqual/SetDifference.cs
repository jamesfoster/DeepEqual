namespace DeepEqual;

public record SetDifference(string Breadcrumb, ImmutableList<object> Expected, ImmutableList<object> Extra) : Difference(Breadcrumb)
{
	public SetDifference(string breadcrumb, IEnumerable<object> expected, IEnumerable<object> extra)
		: this (breadcrumb, expected.ToImmutableList(), extra.ToImmutableList())
	{ }
}