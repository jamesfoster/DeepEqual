namespace DeepEqual
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class ComplexObjectComparison : IComparison
	{
		public IComparison Inner { get; set; }

		public ComplexObjectComparison(IComparison inner)
		{
			Inner = inner;
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return type1.IsClass && type2.IsClass;
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			var props1 = ReflectionCache.GetProperties(value1);
			var props2 = ReflectionCache.GetProperties(value2).ToDictionary(p => p.Name);

			var results = new List<ComparisonResult>();

			foreach (var propertyInfo1 in props1)
			{
				if (!props2.ContainsKey(propertyInfo1.Name))
					continue;

				var propertyInfo2 = props2[propertyInfo1.Name];

				var propValue1 = propertyInfo1.GetValue(value1);
				var propValue2 = propertyInfo2.GetValue(value2);

				var innerContext = context.VisitingProperty(propertyInfo1.Name);
				results.Add(Inner.Compare(innerContext, propValue1, propValue2));
			}

			return results.ToResult();
		}
	}
}