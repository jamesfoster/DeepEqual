namespace DeepEquals.Test
{
	using System.Collections.Generic;

	using Xunit;

	using Syntax;

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
							X = 1,
							Y = 2,
							Z = 3
						},
					Set = new[] {3, 2, 4, 1}
				};

			var object2 = new
				{
					A = 1,
					B = "Absolute",
					C = new[] {1, 2, 3},
					Inner = new TestType
						{
							X = 1,
							Y = 2,
							Z = 3
						},
					Set = new HashSet<int> {1, 2, 3, 4}
				};

			object1.ShouldDeepEqual(object2);
		}
	}

	public class TestType 
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
	}
}