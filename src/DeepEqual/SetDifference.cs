﻿namespace DeepEqual
{
	using System.Collections.Generic;

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
	}
}