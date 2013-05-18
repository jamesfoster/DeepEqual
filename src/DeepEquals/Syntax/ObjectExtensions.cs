namespace DeepEquals.Syntax
{
	using System;
	using System.Linq;

	public static class ObjectExtensions
	{
		 public static bool DeepEquals(this object actual, object expected)
		 {
			 return DeepComparison.CreateComparer().Equals(actual, expected);
		 }

		public static void ShouldDeepEqual(this object actual, object expected)
		{
			var comparison = DeepComparison.Create();

			var context = new ComparisonContext();

			var result = comparison.Compare(context, actual, expected);

			if(result != ComparisonResult.Fail)
				return;

			var message = "Failed";

			if (context.Differences.Count > 0)
			{
				message += ": The following differences were found.";

				message = context.Differences.Aggregate(message, (m, d) => m + "\n\t" + d);
			}

			throw new Exception(message);
		}
	}
}