namespace DeepEqual
{
	using System;

	public class PropertyReader
	{
		public string Name { get; set; }
		public Func<object, object> Read { get; set; }
		public Type DeclaringType { get; set; }
	}
}