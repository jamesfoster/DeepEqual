namespace DeepEqual
{
	using System;

	public class EnumComparison : IComparison
	{
		public bool CanCompare(Type type1, Type type2)
		{
			if (!type1.IsEnum && !type2.IsEnum)
				return false;

			return
				(type1.IsEnum || type1 == typeof(string) || type1 == typeof(int)) &&
				(type2.IsEnum || type2 == typeof(string) || type2 == typeof(int));
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			var value1IsEnum = value1.GetType().IsEnum;
			var value2IsEnum = value2.GetType().IsEnum;

			if (value1IsEnum && value2IsEnum)
			{
				if (value1.ToString() == value2.ToString())
					return ComparisonResult.Pass;

				context.AddDifference(value1, value2);
				return ComparisonResult.Fail;
			}

			var result = value1IsEnum
				? CompareEnumWithConversion(value1, value2)
				: CompareEnumWithConversion(value2, value1);

			if (!result)
				context.AddDifference(value1, value2);

			return result ? ComparisonResult.Pass : ComparisonResult.Fail;
		}

		private static bool CompareEnumWithConversion(object value1, object value2)
		{
			var type = value1.GetType();

			try
			{
				if (value2 is int i)
					value2 = Enum.ToObject(type, i);

				if (value2 is string s)
					value2 = Enum.Parse(type, s);
			}
			catch (Exception)
			{
				return false;
			}

			return value1.Equals(value2);
		}
	}
}