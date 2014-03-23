namespace DeepEqual
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class ComplexObjectComparer
	{
		private readonly IComparison inner;
		private readonly bool ignoreUnmatchedProperties;
		private readonly IDictionary<Type, List<string>> ignoredProperties;

		private readonly List<ComparisonResult> results;
		private Type sourceType;
		private Type destinationType;
		private List<PropertyPair> propertyMap;
		private PropertyPair currentPair;

		public ComplexObjectComparer(IComparison inner, bool ignoreUnmatchedProperties, IDictionary<Type, List<string>> ignoredProperties)
		{
			this.inner = inner;
			this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;
			this.ignoredProperties = ignoredProperties;
			results = new List<ComparisonResult>();
		}

		public ComparisonResult CompareObjects(IComparisonContext context, object source, object destination)
		{
			sourceType = source.GetType();
			destinationType = destination.GetType();

			PreparePropertyInfo(source, destination);


			foreach (var pair in propertyMap)
			{
				currentPair = pair;
				var sourceValue = new Lazy<object>(() => currentPair.Source.Read(source));
				var destinationValue = new Lazy<object>(() => currentPair.Destination.Read(destination));

				if (IsPropertyIgnored())
				{
					continue;
				}

				if (HandleMissingValues(context, destinationValue, sourceValue))
				{
					continue;
				}

				var innerContext = context.VisitingProperty(currentPair.Name);
				results.Add(inner.Compare(innerContext, sourceValue.Value, destinationValue.Value));
			}

			return results.ToResult();
		}

		private bool HandleMissingValues(IComparisonContext context, Lazy<object> destinationValue, Lazy<object> sourceValue)
		{
			if (SourceAndDestinationPresent())
			{
				return false;
			}

			if (ignoreUnmatchedProperties)
			{
				return true;
			}

			if (currentPair.Source == null)
			{
				context.AddDifference("(missing)", destinationValue.Value, currentPair.Name);
				results.Add(ComparisonResult.Fail);
			}

			if (currentPair.Destination == null)
			{
				context.AddDifference(sourceValue.Value, "(missing)", currentPair.Name);
				results.Add(ComparisonResult.Fail);
			}

			return true;
		}

		private bool SourceAndDestinationPresent()
		{
			return currentPair.Source != null && currentPair.Destination != null;
		}

		private void PreparePropertyInfo(object source, object destination)
		{
			var sourceProperties = ReflectionCache.GetProperties(source);
			var destinationProperties = ReflectionCache.GetProperties(destination).ToDictionary(x => x.Name);

			propertyMap = new List<PropertyPair>();

			foreach (var property in sourceProperties)
			{
				var name = property.Name;

				if (destinationProperties.ContainsKey(name))
				{
					propertyMap.Add(new PropertyPair(property, destinationProperties[name], name));
					destinationProperties.Remove(name);
				}
				else
				{
					propertyMap.Add(new PropertyPair(property, null, name));
				}
			}

			foreach (var property in destinationProperties.Values)
			{
				propertyMap.Add(new PropertyPair(null, property, property.Name));
			}
		}

		private bool IsPropertyIgnored()
		{
			var ignored = GetIgnoredPropertiesForTypes(sourceType, destinationType);

			return ignored.Contains(currentPair.Name);
		}

		private List<string> GetIgnoredPropertiesForTypes(Type type1, Type type2)
		{
			var seed = Enumerable.Empty<string>();

			return ignoredProperties
				.Where(pair => pair.Key.IsAssignableFrom(type1) || pair.Key.IsAssignableFrom(type2))
				.Aggregate(seed, (current, pair) => current.Union(pair.Value))
				.ToList();
		}

		private class PropertyPair
		{
			public PropertyPair(ReflectionCache.PropertyReader source, ReflectionCache.PropertyReader destination, string name)
			{
				Source = source;
				Destination = destination;
				Name = name;
			}

			public ReflectionCache.PropertyReader Source { get; private set; }
			public ReflectionCache.PropertyReader Destination { get; private set; }
			public string Name { get; private set; }
		}
	}
}