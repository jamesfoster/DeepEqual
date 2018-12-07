namespace DeepEqual.Formatting
{
	using System;
	using System.Text;

	public class DeepEqualExceptionMessageBuilder
	{
		private readonly IComparisonContext context;
		private readonly IDifferenceFormatterFactory formatterFactory;

		public DeepEqualExceptionMessageBuilder(IComparisonContext context, IDifferenceFormatterFactory formatterFactory)
		{
			this.context = context;
			this.formatterFactory = formatterFactory;
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
			differenceString = differenceString
				.Replace(Environment.NewLine, "\n");
			return string.Join("\n\t\t", differenceString.Split(new[] {"\n"}, StringSplitOptions.None));
		}

		private string FormatDifference(Difference difference)
		{
			var formatter = formatterFactory.GetFormatter(difference);

			return formatter.Format(difference);
		}
	}
}