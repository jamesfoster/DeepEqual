namespace DeepEqual.Syntax
{
	using System;
	using System.Linq;
	using System.Text;

	public static class ObjectExtensions
	{
		public static bool IsDeepEqual(this object actual, object expected)
		{
			var comparison = ComparisonSettings.Create();

			var context = new ComparisonContext();

			var result = comparison.Compare(context, actual, expected);

			return result == ComparisonResult.Pass;
		}

		public static void ShouldDeepEqual(this object actual, object expected)
		{
			var comparison = ComparisonSettings.Create();

			var context = new ComparisonContext();

			var result = comparison.Compare(context, actual, expected);

			if (result != ComparisonResult.Fail)
			{
				return;
			}

			var sb = new StringBuilder();

			sb.Append("Comparison Failed");

			if (context.Differences.Count > 0)
			{
				sb.Append(": The following differences were found.");

				foreach (var difference in context.Differences)
				{
					var lines = difference.ToString().Split(new[] {"\n"}, StringSplitOptions.None);

					sb.Append("\n\t");
					sb.Append(string.Join("\n\t", lines));
				}
			}

			throw new Exception(sb.ToString());
		}
	}
}