namespace DeepEqual.Benchmark
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	using Syntax;

	using Test;

	class Program
	{
		static void Main(string[] args)
		{
						var object1 = new
				{
					Int = 1L,
					Enum = System.UriKind.Absolute,
					List = new List<int> {1, 2, 3},
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
					Int = 1,
					Enum = "Absolute",
					List = new[] {1, 2, 3},
					Inner = new TestType
						{
							X = 1,
							Y = 2,
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

			var sw = new Stopwatch();

			for (int i = 0; i < 10; i++)
			{
				sw.Restart();
				
				for (int j = 0; j < 10000; j++)
				{
					object1.ShouldDeepEqual(object2);
				}

				sw.Stop();

				Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
			}
		}
	}
}
