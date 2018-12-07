namespace DeepEqual.Syntax
{
	using System.Diagnostics.Contracts;

	using DeepEqual.Formatting;

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
			ShouldDeepEqual(actual, expected, null, null);
		}

		public static void ShouldDeepEqual(this object actual, object expected, IComparison comparison)
		{
			ShouldDeepEqual(actual, expected, comparison, null);
		}

		public static void ShouldDeepEqual(
			this object actual,
			object expected,
			IComparison comparison,
			IDifferenceFormatterFactory formatterFactory)
		{
			var builder = new ComparisonBuilder();

			comparison = comparison ?? builder.Create();
			formatterFactory = formatterFactory ?? builder.GetFormatterFactory();

			var context = new ComparisonContext();

			var (result, newContext) = comparison.Compare(context, actual, expected);

			if (result != ComparisonResult.Fail)
			{
				return;
			}

			var message = new DeepEqualExceptionMessageBuilder(newContext, formatterFactory).GetMessage();

			throw new DeepEqualException(message, newContext);
		}

		[Pure]
		public static CompareSyntax<TActual, TExpected> WithDeepEqual<TActual, TExpected>(
			this TActual actual,
			TExpected expected)
		{
			return new CompareSyntax<TActual, TExpected>(actual, expected);
		}
	}
}