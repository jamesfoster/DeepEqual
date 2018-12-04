namespace DeepEqual
{
	using System.Collections.Generic;

	public class SetDifference : Difference
	{
		public List<object> Expected { get; set; }
		public List<object> Extra { get; set; }
	}
}