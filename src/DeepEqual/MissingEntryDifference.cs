namespace DeepEqual
{
	public class MissingEntryDifference : Difference
	{
		public override string ToString()
		{
			return string.Format("Expected{1} not found (Actual{1} = {0})", Prettify(Value1), Breadcrumb);
		}
	}
}