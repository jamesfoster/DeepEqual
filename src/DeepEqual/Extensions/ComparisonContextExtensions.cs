namespace DeepEqual
{
	public static class ComparisonContextExtensions
	{
		public static void AddDifference(this IComparisonContext context, object value1, object value2)
		{
			AddDifference(context, value1, value2, null);
		}

		public static void AddDifference(this IComparisonContext context, object value1, object value2, string childProperty)
		{
			context.AddDifference(new BasicDifference
				{
					Breadcrumb = context.Breadcrumb,
					Value1 = value1,
					Value2 = value2,
					ChildProperty = childProperty
				});
		}
	}
}