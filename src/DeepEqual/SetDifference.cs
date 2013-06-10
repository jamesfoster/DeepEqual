namespace DeepEqual
{
	using System.Collections.Generic;
	using System.Text;

	public class SetDifference : Difference
	{
		public List<object> Expected { get; set; }
		public List<object> Extra { get; set; }

		public SetDifference(string breadcrumb, List<object> expected, List<object> extra)
		{
			Breadcrumb = breadcrumb;
			Expected = expected;
			Extra = extra;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendFormat("Actual{0} != Expected{0}", Breadcrumb);

			if (Extra.Count != 0)
			{
				sb.AppendLine();
				sb.AppendFormat("  Actual{0} contains the following unmatched elements", Breadcrumb);
				foreach (var o in Extra)
				{
					sb.AppendLine();
					sb.AppendFormat("\t{0}", Prettify(o));
				}
			}

			if (Expected.Count != 0)
			{
				sb.AppendLine();
				sb.AppendFormat("  Expected{0} contains the following unmatched elements", Breadcrumb);
				foreach (var o in Expected)
				{
					sb.AppendLine();
					sb.AppendFormat("\t{0}", Prettify(o));
				}
			}

			return sb.ToString();
		}
	}
}