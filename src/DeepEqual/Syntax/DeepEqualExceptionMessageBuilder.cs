namespace DeepEqual.Syntax
{
	using System;
	using System.Text;

	public class DeepEqualExceptionMessageBuilder
	{
		private readonly ComparisonContext context;

		public DeepEqualExceptionMessageBuilder(ComparisonContext context)
		{
			this.context = context;
		}

		public string GetMessage()
		{
			var sb = new StringBuilder();

			sb.Append("Comparison Failed");

			if (context.Differences.Count > 0)
			{
				sb.AppendFormat(": The following {0} differences were found.", context.Differences.Count);

				foreach (var difference in context.Differences)
				{
					var lines = difference.ToString().Split(new[] {"\n"}, StringSplitOptions.None);

					sb.Append("\n\t");
					sb.Append(String.Join("\n\t", lines));
				}
			}

			return sb.ToString();
		}
	}
}