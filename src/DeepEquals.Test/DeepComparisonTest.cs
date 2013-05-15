namespace DeepEquals.Test
{
	using System.Collections.Generic;

	using Xunit;

	public class DeepComparisonTest
	{
		[Fact]
		public void Test()
		{
			var object1 = new
				{
					A = 1,
					B = System.UriKind.Absolute,
					C = new List<int> {1, 2, 3},
					Inner = new
						{
							X = 1, Y = 2, Z = 3
						}
				};

			var object2 = new
				{
					A = 1,
					B = "Absolute",
					C = new[] {1, 2, 3},
					Inner = new TestType
						{
							X = 1, Y = 2, Z = 3
						}
				};
			
			var comparer = new DeepComparison().ToDeepComparer();

			var result = comparer.Equals(object1, object2);

			Assert.True(result);
		}
	}

	public class TestType 
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
	}
}