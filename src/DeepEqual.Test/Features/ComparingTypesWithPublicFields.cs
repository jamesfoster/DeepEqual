namespace DeepEqual.Test.Features
{
	using DeepEqual.Test.Helper;

	using Xunit;

	public class ComparingTypesWithPublicFields
	{
		[Fact]
		public static void Should_consider_public_fields_when_comparing_complex_objects()
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

			DeepAssert.AreNotEqual(actual, expected);
		}

		public class Data
		{
			public int Id;
			public string Name { get; set; }
		}
	}
}