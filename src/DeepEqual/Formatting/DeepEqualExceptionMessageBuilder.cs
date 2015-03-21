namespace DeepEqual.Formatting
{
	using System;
	using System.Text;

	public class DeepEqualExceptionMessageBuilder
	{
		private readonly ComparisonContext context;
		private readonly DifferenceFormatterFactory formatterFactory;

		public DeepEqualExceptionMessageBuilder(ComparisonContext context)
		{
			this.context = context;
			formatterFactory = new DifferenceFormatterFactory();
		}

		public string GetMessage()
		{
			var sb = new StringBuilder();

			sb.Append("Comparison Failed");

			if (context.Differences.Count > 0)
			{
				sb.AppendFormat(": The following {0} differences were found.", context.Differences.Count);
			}

			foreach (var difference in context.Differences)
			{
				sb.Append("\n\t");

				var text = Indent(FormatDifference(difference));
				sb.Append(text);
			}

			return sb.ToString();
		}

		private static string Indent(string differenceString)
		{
			return string.Join("\n\t\t", differenceString.Split(new[] {"\n"}, StringSplitOptions.None));
		}

		private string FormatDifference(Difference difference)
		{
			var formatter = formatterFactory.GetFormatter(difference);

			return formatter.Format(difference);
		}
	}
}