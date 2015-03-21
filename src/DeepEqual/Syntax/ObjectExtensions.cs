﻿namespace DeepEqual.Syntax
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Text;

	public static class ObjectExtensions
	{
		[Pure]
		public static bool IsDeepEqual(this object actual, object expected, IComparison comparison = null)
		{
			comparison = comparison ?? new ComparisonBuilder().Create();

			var context = new ComparisonContext();

			var result = comparison.Compare(context, actual, expected);

			return result != ComparisonResult.Fail;
		}

		public static void ShouldDeepEqual(this object actual, object expected, IComparison comparison = null)
		{
			comparison = comparison ?? new ComparisonBuilder().Create();

			var context = new ComparisonContext();

			var result = comparison.Compare(context, actual, expected);

			if (result != ComparisonResult.Fail)
			{
				return;
			}

			throw new DeepEqualException(context);
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