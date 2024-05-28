namespace DeepEqual;

public class ComplexObjectComparer
{
	private readonly IComparison inner;
	private readonly bool ignoreUnmatchedProperties;
	private readonly List<Func<PropertyReader, bool>> ignoredProperties;

	private readonly List<ComparisonResult> results;
	private List<PropertyPair> propertyMap = [];

	public ComplexObjectComparer(
		IComparison inner,
		bool ignoreUnmatchedProperties,
		List<Func<PropertyReader, bool>> ignoredProperties
	)
	{
		this.inner = inner;
		this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;
		this.ignoredProperties = ignoredProperties;
		results = [];
	}

	public (ComparisonResult, IComparisonContext) CompareObjects(
		IComparisonContext context,
		object? source,
		object? destination
	)
	{
		if (source == null || destination == null)
		{
			return (ComparisonResult.Inconclusive, context);
		}

		PreparePropertyInfo(source, destination);

		if (propertyMap.Count == 0) return (ComparisonResult.Pass, context);

		var currentContext = context;

		foreach (var pair in propertyMap)
		{
			var sourceValue = new Lazy<object?>(() => pair.Source!.Read(source));
			var destinationValue = new Lazy<object?>(() => pair.Destination!.Read(destination));

			if (IsPropertyIgnored(pair))
			{
				continue;
			}

			if (SourceAndDestinationPresent(pair))
			{
				var (result, innerContext) = inner.Compare(
					context.VisitingProperty(pair.Name),
					sourceValue.Value,
					destinationValue.Value
				);

				results.Add(result);
				currentContext = currentContext.MergeDifferencesFrom(innerContext);
			}
			else if (!ignoreUnmatchedProperties)
			{
				if (pair.Source == null)
				{
					currentContext = currentContext.AddDifference("(missing)", destinationValue.Value, pair.Name);
					results.Add(ComparisonResult.Fail);
				}

				if (pair.Destination == null)
				{
					currentContext = currentContext.AddDifference(sourceValue.Value, "(missing)", pair.Name);
					results.Add(ComparisonResult.Fail);
				}
			}

		}

		return (results.ToResult(), currentContext);
	}

	private bool SourceAndDestinationPresent(PropertyPair pair)
	{
		return pair.Source != null && pair.Destination != null;
	}

	private void PreparePropertyInfo(object source, object destination)
	{
		var sourceProperties = ReflectionCache
			.GetProperties(source);

		var destinationProperties = ReflectionCache
			.GetProperties(destination)
			.ToDictionary(x => x.Name);

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

	private bool IsPropertyIgnored(PropertyPair pair)
	{
		foreach (var ignoredProperty in ignoredProperties)
		{
			if (pair.Source != null && ignoredProperty(pair.Source))
			{
				return true;
			}

			if (pair.Destination != null && ignoredProperty(pair.Destination))
			{
				return true;
			}
		}

		return false;
	}

	private class PropertyPair
	{
		public PropertyPair(PropertyReader? source, PropertyReader? destination, string name)
		{
			Source = source;
			Destination = destination;
			Name = name;
		}

		public PropertyReader? Source { get; }
		public PropertyReader? Destination { get; }
		public string Name { get; }
	}
}