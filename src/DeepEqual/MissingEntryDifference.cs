namespace DeepEqual
{
	public class MissingEntryDifference : Difference
	{
		public override string ToString()
		{
			return string.Format("Expected{0}[{1}] not found (Actual{0}[{1}] = {2})",
				Breadcrumb,
				Prettify(Value1),
				Prettify(Value2));
		}
	}
}