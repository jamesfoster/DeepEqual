namespace DeepEqual.Test.Helper
{
	using System;

	using DeepEqual.Syntax;

	using Shouldly;

	using Xunit;

	public static class DeepAssert
	{
		public static void AreEqual(object actual, object expected, IComparison comparison = null)
		{
			actual.IsDeepEqual(expected, comparison).ShouldBe(true);
			actual.ShouldDeepEqual(expected, comparison);
		}

		public static void AreNotEqual(object actual, object expected, IComparison comparison = null)
		{
			actual.IsDeepEqual(expected, comparison).ShouldBe(false);
			Assert.Throws<Exception>(() => actual.ShouldDeepEqual(expected, comparison));
		}
	}
}