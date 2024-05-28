namespace DeepEqual;

public class ComplexObjectComparer
{
    private readonly IComparison inner;
    private readonly bool ignoreUnmatchedProperties;
    private readonly List<Func<PropertyReader, bool>> ignoredProperties;
    private readonly List<Func<Type, Type, string, string?>> mappedProperties;

    private readonly List<ComparisonResult> results;

    public ComplexObjectComparer(
        IComparison inner,
        bool ignoreUnmatchedProperties,
        List<Func<PropertyReader, bool>> ignoredProperties,
        List<Func<Type, Type, string, string?>> mappedProperties
    )
    {
        this.inner = inner;
        this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;
        this.ignoredProperties = ignoredProperties;
        this.mappedProperties = mappedProperties;
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

        var propertyMap = PreparePropertyInfo(source, destination);

        if (propertyMap.Count == 0)
            return (ComparisonResult.Pass, context);

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
                    context.VisitingProperty(pair.SourceName, pair.DestinationName),
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
                    currentContext = currentContext.AddDifference(
                        "(missing)",
                        destinationValue.Value,
                        pair.SourceName,
                        pair.DestinationName
                    );
                    results.Add(ComparisonResult.Fail);
                }

                if (pair.Destination == null)
                {
                    currentContext = currentContext.AddDifference(
                        sourceValue.Value,
                        "(missing)",
                        pair.SourceName,
                        pair.DestinationName
                    );
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

    private List<PropertyPair> PreparePropertyInfo(object source, object destination)
    {
        var sourceType = source.GetType();
        var destinationType = destination.GetType();

        var sourceProperties = ReflectionCache.GetProperties(source);

        var destinationProperties = ReflectionCache
            .GetProperties(destination)
            .ToDictionary(x => x.Name);

        var propertyMap = new List<PropertyPair>();

        foreach (var property in sourceProperties)
        {
            var sourceName = property.Name;
            var destinationName = GetMappedPropertyName(sourceType, destinationType, sourceName);

            if (destinationProperties.ContainsKey(destinationName))
            {
                propertyMap.Add(
                    new PropertyPair(
                        property,
                        destinationProperties[destinationName],
                        sourceName,
                        destinationName
                    )
                );
                destinationProperties.Remove(destinationName);
            }
            else
            {
                propertyMap.Add(new PropertyPair(property, null, sourceName, destinationName));
            }
        }

        foreach (var property in destinationProperties.Values)
        {
            propertyMap.Add(new PropertyPair(null, property, property.Name, property.Name));
        }

        return propertyMap;
    }

    private string GetMappedPropertyName(Type sourceType, Type destinationType, string sourceName)
    {
        foreach (var map in mappedProperties)
        {
            var result = map(sourceType, destinationType, sourceName);
            if (result != null)
                return result;
        }
        return sourceName;
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
        public PropertyPair(
            PropertyReader? source,
            PropertyReader? destination,
            string sourceName,
            string destinationName
        )
        {
            Source = source;
            Destination = destination;
            SourceName = sourceName;
            DestinationName = destinationName;
        }

        public PropertyReader? Source { get; }
        public PropertyReader? Destination { get; }
        public string SourceName { get; }
        public string DestinationName { get; }
    }
}
