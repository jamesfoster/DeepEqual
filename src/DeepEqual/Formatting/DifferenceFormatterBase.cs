namespace DeepEqual.Formatting
{
	public abstract class DifferenceFormatterBase : IDifferenceFormatter
	{
		public abstract string Format(Difference difference);

		protected static string Prettify(object value)
		{
			if (value == null)
				return "(null)";

			if (value is string)
				return string.Format("\"{0}\"", value);

			return value.ToString();
		}
	}
}