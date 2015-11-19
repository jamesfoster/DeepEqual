namespace DeepEqual.Formatting
{
	using System;
	using System.Linq;

	public class BasicDifferenceFormatter : DifferenceFormatterBase
	{
		private const int InitialMaxLength = 20;

		public override string Format(Difference difference)
		{
			var format = "Actual{0}.{1} != Expected{0}.{1} ({2} != {3})";

			if (difference.ChildProperty == null)
				format = "Actual{0} != Expected{0} ({2} != {3})";

			var value1 = difference.Value1;
			var value2 = difference.Value2;

			if (value1 is string && value2 is string)
				FixLongStringDifference(ref value1, ref value2);

			return string.Format(
				format,
				difference.Breadcrumb,
				difference.ChildProperty,
				Prettify(value1),
				Prettify(value2));
		}

		private void FixLongStringDifference(ref object v1, ref object v2)
		{
			var maxLength1 = InitialMaxLength;
			var maxLength2 = InitialMaxLength;

			var value1 = (string) v1;
			var value2 = (string) v2;

			var firstDiffIndex = FindIndexOfFirstDifferentChar(value1, value2);

			var lowerBound = Math.Max(0, firstDiffIndex - 10);

			if (lowerBound >= 3)
			{
				if (value1.Length > maxLength1)
				{
					value1 = "..." + value1.Substring(Math.Min(lowerBound, value1.Length - maxLength1));
					maxLength1 += 3;
				}

				if (value2.Length > maxLength2)
				{
					value2 = "..." + value2.Substring(Math.Min(lowerBound, value2.Length - maxLength2));
					maxLength2 += 3;
				}
			}

			if (value1.Length > maxLength1 + 3) value1 = value1.Substring(0, maxLength1) + "...";
			if (value2.Length > maxLength2 + 3) value2 = value2.Substring(0, maxLength2) + "...";

			v1 = value1;
			v2 = value2;
		}

		private int FindIndexOfFirstDifferentChar(string value1, string value2)
		{
			var numEqualChars = value1.Zip(value2, (a, b) => a == b).TakeWhile(x => x).Count();

			return numEqualChars;
		}
	}
}