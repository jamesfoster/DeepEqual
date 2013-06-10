namespace DeepEqual
{
	using System;

	public class ComparisonSettings
	{
		static ComparisonSettings()
		{
			Create = () => new ComparisonBuilder().Create();
		}

		public static Func<IComparison> Create { get; set; }
	}
}