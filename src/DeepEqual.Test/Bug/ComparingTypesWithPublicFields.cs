namespace DeepEqual.Test.Bug
{
	using DeepEqual.Syntax;

	using Shouldly;

	using Xunit;

	public class ComparingTypesWithPublicFields
	{
		[Fact]
		public static void TestIgnoreByType()
		{
			var expected = new Data
				{
					Id = 1,
					Name = "Joe"
				};

			var actual = new Data
				{
					Id = 2,
					Name = "Joe"
				};

			actual.IsDeepEqual(expected).ShouldBe(false);
		}

		public class Data
		{
			public int Id;
			public string Name { get; set; }
		}
	}
}