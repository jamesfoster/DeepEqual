namespace DeepEqual;

public class ComplexObjectComparer
{
	private readonly IComparison inner;
	private readonly bool ignoreUnmatchedProperties;
	private readonly List<Func<PropertyReader, bool>> ignoredProperties;

	private readonly List<ComparisonResult> results;
	private List<PropertyPair> propertyMap;
	private PropertyPair currentPair;

	public ComplexObjectComparer(
		IComparison inner,
		bool ignoreUnmatchedProperties,
		List<Func<PropertyReader, bool>> ignoredProperties
	)
	{
		this.inner = inner;
		this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;
		this.ignoredProperties = ignoredProperties;
		results = new List<ComparisonResult>();
	}

	public (ComparisonResult, IComparisonContext) CompareObjects(
		IComparisonContext context,
		object source,
		object destination
	)
	{
		PreparePropertyInfo(source, destination);

		if (propertyMap.Count == 0) return (ComparisonResult.Pass, context);

		var currentContext = context;

		foreach (var pair in propertyMap)
		{
			currentPair = pair;
			var sourceValue = new Lazy<object>(() => currentPair.Source.Read(source));
			var destinationValue = new Lazy<object>(() => currentPair.Destination.Read(destination));

			if (IsPropertyIgnored())
			{
				continue;
			}

			if (SourceAndDestinationPresent())
			{
				var (result, innerContext) = inner.Compare(
					context.VisitingProperty(currentPair.Name),
					sourceValue.Value,
					destinationValue.Value
				);

				results.Add(result);
				currentContext = currentContext.MergeDifferencesFrom(innerContext);
			}
			else if (!ignoreUnmatchedProperties)
			{
				if (currentPair.Source == null)
				{
					currentContext = currentContext.AddDifference("(missing)", destinationValue.Value, currentPair.Name);
					results.Add(ComparisonResult.Fail);
				}

				if (currentPair.Destination == null)
				{
					currentContext = currentContext.AddDifference(sourceValue.Value, "(missing)", currentPair.Name);
					results.Add(ComparisonResult.Fail);
				}
			}

		}

		return (results.ToResult(), currentContext);
	}

	private bool SourceAndDestinationPresent()
	{
		return currentPair.Source != null && currentPair.Destination != null;
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

	private bool IsPropertyIgnored()
	{
		foreach (var ignoredProperty in ignoredProperties)
		{
			if (currentPair.Source != null && ignoredProperty(currentPair.Source))
			{
				return true;
			}

			if (currentPair.Destination != null && ignoredProperty(currentPair.Destination))
			{
				return true;
			}
		}

		return false;
	}

	private class PropertyPair
	{
		public PropertyPair(PropertyReader source, PropertyReader destination, string name)
		{
			Source = source;
			Destination = destination;
			Name = name;
		}

		public PropertyReader Source { get; }
		public PropertyReader Destination { get; }
		public string Name { get; }
	}
}