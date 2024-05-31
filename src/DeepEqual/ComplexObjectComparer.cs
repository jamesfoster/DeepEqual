namespace DeepEqual;

public class ComplexObjectComparer
{
    private readonly bool ignoreUnmatchedProperties;
    private readonly IReadOnlyList<Func<PropertyPair, bool>> ignoredProperties;
    private readonly IReadOnlyList<Func<Type, Type, string, string?>> mappedProperties;

    private readonly List<ComparisonResult> results;

    public ComplexObjectComparer(
        bool ignoreUnmatchedProperties,
        IReadOnlyList<Func<PropertyPair, bool>> ignoredProperties,
        IReadOnlyList<Func<Type, Type, string, string?>> mappedProperties
    )
    {
        this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;
        this.ignoredProperties = ignoredProperties;
        this.mappedProperties = mappedProperties;
        results = [];
    }

    public (ComparisonResult, IComparisonContext) CompareObjects(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        if (leftValue == null || rightValue == null)
        {
            return (ComparisonResult.Inconclusive, context);
        }

        var propertyMap = PreparePropertyInfo(leftValue, rightValue);

        if (propertyMap.Count == 0)
            return (ComparisonResult.Pass, context);

        var currentContext = context;

        foreach (var pair in propertyMap)
        {
            var leftPropValue = new Lazy<object?>(() => pair.Left!.Read(leftValue));
            var rightPropValue = new Lazy<object?>(() => pair.Right!.Read(rightValue));

            if (IsPropertyIgnored(pair))
            {
                continue;
            }

            if (LeftAndRightPresent(pair))
            {
                var (result, innerContext) = context
                    .VisitingProperty(pair.LeftName, pair.RightName)
                    .Compare(leftPropValue.Value, rightPropValue.Value);

                results.Add(result);
                currentContext = currentContext.MergeDifferencesFrom(innerContext);
            }
            else if (!ignoreUnmatchedProperties)
            {
                if (pair.Left == null)
                {
                    currentContext = currentContext.AddDifference(
                        "(missing)",
                        rightPropValue.Value,
                        pair.LeftName,
                        pair.RightName
                    );
                    results.Add(ComparisonResult.Fail);
                }

                if (pair.Right == null)
                {
                    currentContext = currentContext.AddDifference(
                        leftPropValue.Value,
                        "(missing)",
                        pair.LeftName,
                        pair.RightName
                    );
                    results.Add(ComparisonResult.Fail);
                }
            }
        }

        return (results.ToResult(), currentContext);
    }

    private bool LeftAndRightPresent(PropertyPair pair)
    {
        return pair.Left != null && pair.Right != null;
    }

    private List<PropertyPair> PreparePropertyInfo(object leftValue, object rightValue)
    {
        var leftType = leftValue.GetType();
        var rightType = rightValue.GetType();

        var leftProperties = ReflectionCache.GetProperties(leftValue);

        var rightProperties = ReflectionCache.GetProperties(rightValue).ToDictionary(x => x.Name);

        var propertyMap = new List<PropertyPair>();

        foreach (var leftProp in leftProperties)
        {
            var leftPropName = leftProp.Name;
            var rightPropName = GetMappedPropertyName(leftType, rightType, leftPropName);

            if (rightProperties.ContainsKey(rightPropName))
            {
                var rightProp = rightProperties[rightPropName];
                propertyMap.Add(new PropertyPair(leftProp, rightProp, leftPropName, rightPropName));
                rightProperties.Remove(rightPropName);
            }
            else
            {
                propertyMap.Add(new PropertyPair(leftProp, null, leftPropName, rightPropName));
            }
        }

        foreach (var property in rightProperties.Values)
        {
            propertyMap.Add(new PropertyPair(null, property, property.Name, property.Name));
        }

        return propertyMap;
    }

    private string GetMappedPropertyName(Type leftType, Type rightType, string leftPropName)
    {
        foreach (var map in mappedProperties)
        {
            var result = map(leftType, rightType, leftPropName);
            if (result != null)
                return result;
        }
        return leftPropName;
    }

    private bool IsPropertyIgnored(PropertyPair pair)
    {
        foreach (var ignoredProperty in ignoredProperties)
        {
            if (ignoredProperty(pair))
            {
                return true;
            }
        }

        return false;
    }
}

public class PropertyPair
{
    public PropertyPair(
        PropertyReader? left,
        PropertyReader? right,
        string leftName,
        string rightName
    )
    {
        Left = left;
        Right = right;
        LeftName = leftName;
        RightName = rightName;
    }

    public PropertyReader? Left { get; }
    public PropertyReader? Right { get; }
    public string LeftName { get; }
    public string RightName { get; }
}
