using DeepEqual.Test.Helper;

using Xunit;

namespace DeepEqual.Test.Features;

public class ComparingTypesWithPrivateFields
{

	[Fact]
	public static void When_exposing_internals_differences_in_internals_count()
	{
		var expected = new ClassWithPrivates(123)
		{
			Id = 234,
			Name = "Joe"
		};

		var actual = new ClassWithPrivates(321)
		{
			Id = 234,
			Name = "Joe"
		};

		var comparison = new ComparisonBuilder()
			.ExposeInternalsOf<ClassWithPrivates>()
			.Create();

		DeepAssert.AreNotEqual(actual, expected, comparison);
	}

	[Fact]
	public static void When_exposing_internals_similar_objects_are_considered_the_same()
	{
		var expected = new ClassWithPrivates(123)
		{
			Id = 234,
			Name = "Joe"
		};

		var actual = new ClassWithPrivates(123)
		{
			Id = 234,
			Name = "Joe"
		};

		var comparison = new ComparisonBuilder()
			.ExposeInternalsOf<ClassWithPrivates>()
			.Create();

		DeepAssert.AreEqual(actual, expected, comparison);
	}

	public class ClassWithPrivates
	{
		public int Id;
		private readonly int privateInt;

		public ClassWithPrivates(int privateInt)
		{
			this.privateInt = privateInt;
		}

		public string Name { get; set; }

	}
}