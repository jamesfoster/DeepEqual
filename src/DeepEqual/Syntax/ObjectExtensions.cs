namespace DeepEqual.Syntax
{
	using System.Diagnostics.Contracts;

	public static class ObjectExtensions
	{
		[Pure]
		public static bool IsDeepEqual(this object actual, object expected)
		{
			return IsDeepEqual(actual, expected, null);
		}

		[Pure]
		public static bool IsDeepEqual(this object actual, object expected, IComparison comparison)
		{
			comparison = comparison ?? new ComparisonBuilder().Create();

			var context = new ComparisonContext();

			var (result, _) = comparison.Compare(context, actual, expected);

			return result != ComparisonResult.Fail;
		}

		public static void ShouldDeepEqual(this object actual, object expected)
		{
			ShouldDeepEqual(actual, expected, null);
		}

		public static void ShouldDeepEqual(this object actual, object expected, IComparison comparison)
		{
			comparison = comparison ?? new ComparisonBuilder().Create();

			var context = new ComparisonContext();

			var (result, newContext) = comparison.Compare(context, actual, expected);

			if (result != ComparisonResult.Fail)
			{
				return;
			}

			throw new DeepEqualException(newContext);
		}

		[Pure]
		public static CompareSyntax<TActual, TExpected> WithDeepEqual<TActual, TExpected>(
			this TActual actual,
			TExpected expected
		)
		{
			return new CompareSyntax<TActual, TExpected>(actual, expected);
		}
	}
}