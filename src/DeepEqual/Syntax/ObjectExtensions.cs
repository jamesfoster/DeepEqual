namespace DeepEqual.Syntax
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Text;

	public static class ObjectExtensions
	{
		[Pure]
		public static bool IsDeepEqual(this object actual, object expected, IComparison comparison = null)
		{
			comparison = comparison ?? new ComparisonBuilder().Create();

			var context = new ComparisonContext();

			var result = comparison.Compare(context, actual, expected);

			return result != ComparisonResult.Fail;
		}

		public static void ShouldDeepEqual(this object actual, object expected, IComparison comparison = null)
		{
			comparison = comparison ?? new ComparisonBuilder().Create();

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
				sb.AppendFormat(": The following {0} differences were found.", context.Differences.Count);

				foreach (var difference in context.Differences)
				{
					var lines = difference.ToString().Split(new[] {"\n"}, StringSplitOptions.None);

					sb.Append("\n\t");
					sb.Append(string.Join("\n\t", lines));
				}
			}

			throw new Exception(sb.ToString());
		}

		[Pure]
		public static CompareSyntax<TActual, TExpected> WithDeepEqual<TActual, TExpected>(
			this TActual actual,
			TExpected expected)
		{
			return new CompareSyntax<TActual, TExpected>(actual, expected);
		}
	}
}