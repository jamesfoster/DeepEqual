namespace DeepEqual.Test
{
    using System.Collections.Generic;

	using Xunit;
    using Xunit.Extensions;

	using Syntax;
    
    using Xbehave;
    using Shouldly;
   	using System.Collections.Generic;

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
					Set = new[] {3, 4, 2, 1},
					Dictionary = new Dictionary<int, int>
						{
							{2, 3},
							{123, 234},
							{345, 456}
						}
				};

			var object2 = new
				{
					A = 1,
					B = "Absolute",
					C = new[] {1, 2, 3},
					Inner = new TestType
						{
							X = 1,
							Y = 3,
							Z = 3
						},
					Set = new HashSet<int> {1, 2, 3, 4},
					Dictionary = new Dictionary<int, int>
						{
							{123, 234},
							{345, 456},
							{2, 3}
						}
				};


			object1.WithDeepEqual(object2)
			       .IgnoreProperty<TestType>(x => x.Y)
			       .Assert();

//			object1.ShouldDeepEqual(object2);
        }

        [Scenario]
        public void TestIgnorePropertiesRegardlessOfType()
        {
            var object1 = new
            {
                X = 1,
                Y = 2,
                Z = 3
            };

            var object2 = new
            {
                X = 1,
                Y = 3,
                Z = 3
            };

            bool objectsEqual = false;

            "Given a comparison between 2 objects where Y differs and Y is ignored"
                .Given(() => objectsEqual = object1.WithDeepEqual(object2)
                                                   .IgnoreProperty("Y")
                                                   .Compare());

            "Then it should find they are equal"
                .Then(() => objectsEqual.ShouldBe(true));

            "Given a comparison between 2 objects where Y differs but Z is ignored"
                .Given(() => objectsEqual = object1.WithDeepEqual(object2)
                                                   .IgnoreProperty("Z")
                                                   .Compare());

            "Then it should find they are not equal"
                .Then(() => objectsEqual.ShouldBe(false));
        }

        [Scenario]
        public void TestListOrderSignificant()
        {
            var object1 = new
            {
                X = 1,
                List = new List<TestType> {
                    new TestType { X = 1, Y = 12, Z = 13 },
                    new TestType { X = 2, Y = 22, Z = 23 }
                }
            };

            var object2 = new
            {
                X = 1,
                List = new List<TestType> {
                    new TestType { X = 2, Y = 22, Z = 23 },
                    new TestType { X = 1, Y = 12, Z = 13 }
                }
            };

            bool objectsEqual = object1.WithDeepEqual(object2)
                                                   .Compare();

            "Given a comparison between 2 objects with lists where they only differ in list order"
                .Given(() => { });

            "Then it should find they are not equal"
                .Then(() => objectsEqual.ShouldBe(false));
        }

        [Scenario]
        public void TestDisregardListOrder()
        {
            var object1 = new
            {
                X = 1,
                List = new List<TestType> {
                    new TestType { X = 1, Y = 12, Z = 13 },
                    new TestType { X = 2, Y = 22, Z = 23 }
                }
            };

            var object2 = new
            {
                X = 1,
                List = new List<TestType> {
                    new TestType { X = 2, Y = 22, Z = 23 },
                    new TestType { X = 1, Y = 12, Z = 13 }
                }
            };

            bool objectsEqual = object1.WithDeepEqual(object2)
                                                    .DisregardListOrder()
                                                    .Compare();

            "Given a comparison between 2 objects with lists where they only differ in list order, and DisregardListOrder is used"
                .Given(() => { });

            "Then it should find they are equal"
                .Then(() => objectsEqual.ShouldBe(true));
        }
    }

	public class TestType 
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
	}

}
