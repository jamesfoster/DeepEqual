namespace DeepEqual
{
	public class BasicDifference : Difference
	{
		public string ChildProperty { get; set; }
		public object Value1 { get; set; }
		public object Value2 { get; set; }
	}
}