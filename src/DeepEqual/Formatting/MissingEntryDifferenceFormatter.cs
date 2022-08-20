namespace DeepEqual.Formatting
{
	public class MissingEntryDifferenceFormatter : DifferenceFormatterBase
	{
		public override string Format(Difference diff)
		{
			var difference = (MissingEntryDifference) diff;

			var format =
				difference.Side == MissingSide.Expected
					? "Expected{0}[{1}] not found (Actual{0}[{1}] = {2})"
					: "Actual{0}[{1}] not found (Expected{0}[{1}] = {2})";

			return string.Format(format,
				difference.Breadcrumb,
				Prettify(difference.Key),
				Prettify(difference.Value)
			);
		}
	}
}