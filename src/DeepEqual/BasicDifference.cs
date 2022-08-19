namespace DeepEqual;

public class BasicDifference : Difference
{
	public BasicDifference(string breadcrumb, object value1, object value2, string childProperty) : base(breadcrumb)
	{
		Value1 = value1;
		Value2 = value2;
		ChildProperty = childProperty;
	}

	public object Value1 { get; }
	public object Value2 { get; }
	public string ChildProperty { get; }
}