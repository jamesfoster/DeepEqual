namespace DeepEqual
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	public class ComplexObjectComparison : IComparison
	{
		public IComparison Inner { get; set; }

		public bool IgnoreUnmatchedProperties { get; set; }
        public IgnoredProperties IgnoredProperties { get; private set; }

        public ComplexObjectComparison(IComparison inner, IgnoredProperties ignoredProperties)
		{
			Inner = inner;
            IgnoredProperties = ignoredProperties;
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return type1.IsClass && type2.IsClass;
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			var type1 = value1.GetType();
			var type2 = value2.GetType();

			var props1 = ReflectionCache.GetProperties(value1);
			var props2 = ReflectionCache.GetProperties(value2).ToDictionary(p => p.Name);

            var ignored = new List<string>();
            if (IgnoredProperties != null)
            {
                ignored = IgnoredProperties.GetIgnoredPropertiesForTypes(type1, type2);
            }

			var results = new List<ComparisonResult>();

			foreach (var propertyInfo1 in props1)
			{
				if (ignored.Contains(propertyInfo1.Name))
				{
					props2.Remove(propertyInfo1.Name);
					continue;
				}

				var propValue1 = propertyInfo1.Read(value1);

				if (!props2.ContainsKey(propertyInfo1.Name))
				{
					if (!IgnoreUnmatchedProperties)
					{
						context.AddDifference(propValue1, "(missing)", propertyInfo1.Name);
						results.Add(ComparisonResult.Fail);
					}
					continue;
				}

				var propertyInfo2 = props2[propertyInfo1.Name];
				var propValue2 = propertyInfo2.Read(value2);

				var innerContext = context.VisitingProperty(propertyInfo1.Name);
				results.Add(Inner.Compare(innerContext, propValue1, propValue2));

				props2.Remove(propertyInfo1.Name);
			}

			if (!IgnoreUnmatchedProperties && props2.Count > 0)
			{
				foreach (var p in props2)
				{
					if (ignored.Contains(p.Key))
					{
						continue;
					}

					var v = p.Value.Read(value2);
					context.AddDifference("(missing)", v, p.Key);
					results.Add(ComparisonResult.Fail);
				}
			}

			return results.ToResult();
		}
	}
}
