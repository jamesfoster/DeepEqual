using System.Text.Json;

namespace DeepEqual.SystemTextJson;

public class JsonElementComparison : IComparison
{
    private static readonly Type jsonElementType = typeof(JsonElement);
    private static readonly Type[] allowableTypes = { jsonElementType, typeof(string) };

    public bool CanCompare(Type type1, Type type2)
    {
        return type1 == jsonElementType && allowableTypes.Contains(type2)
            || type2 == jsonElementType && allowableTypes.Contains(type1);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object value1,
        object value2
    )
    {
        var element1 = AsJsonElement(value1);
        var element2 = AsJsonElement(value2);

        if (element1 is null || element2 is null)
            return (ComparisonResult.Inconclusive, context);

        if (element1.Value.ValueKind != element2.Value.ValueKind)
        {
            return (
                ComparisonResult.Fail,
                context.AddDifference(element1.Value.ValueKind, element2.Value.ValueKind, "Kind")
            );
        }

        return element1.Value.ValueKind switch
        {
            JsonValueKind.Undefined => (ComparisonResult.Pass, context),
            JsonValueKind.Object => CompareObject(element1.Value, element2.Value, context),
            JsonValueKind.Array => CompareArray(element1.Value, element2.Value, context),
            JsonValueKind.String => CompareString(element1.Value, element2.Value, context),
            JsonValueKind.Number => CompareNumber(element1.Value, element2.Value, context),
            JsonValueKind.True => (ComparisonResult.Pass, context),
            JsonValueKind.False => (ComparisonResult.Pass, context),
            JsonValueKind.Null => (ComparisonResult.Pass, context),
            _ => throw new NotImplementedException(),
        };
    }

    private (ComparisonResult result, IComparisonContext context) CompareObject(
        JsonElement value1,
        JsonElement value2,
        IComparisonContext context
    )
    {
        var newContext = context;
        var results = new List<ComparisonResult>();

        var properties1 = value1.EnumerateObject().ToDictionary(x => x.Name);
        var properties2 = value2.EnumerateObject().ToDictionary(x => x.Name);

        if (properties1.Count == 0 && properties2.Count == 0)
        {
            return (ComparisonResult.Pass, context);
        }

        foreach (var name in properties1.Keys)
        {
            var prop1 = properties1[name];

            if (properties2.TryGetValue(name, out var prop2))
            {
                var (result, resultContext) = Compare(
                    context.VisitingProperty(name),
                    prop1.Value,
                    prop2.Value
                );
                results.Add(result);
                newContext = newContext.MergeDifferencesFrom(resultContext);
                properties2.Remove(name);
            }
            else
            {
                newContext = newContext.AddDifference(
                    new MissingEntryDifference(
                        context.Breadcrumb,
                        MissingSide.Expected,
                        name,
                        prop1.Value
                    )
                );
                results.Add(ComparisonResult.Fail);
            }
        }

        foreach (var name in properties2.Keys)
        {
            var prop2 = properties2[name];

            newContext = newContext.AddDifference(
                new MissingEntryDifference(
                    context.Breadcrumb,
                    MissingSide.Actual,
                    name,
                    prop2.Value
                )
            );
            results.Add(ComparisonResult.Fail);
        }

        return (results.ToResult(), newContext);
    }

    private (ComparisonResult result, IComparisonContext context) CompareArray(
        JsonElement value1,
        JsonElement value2,
        IComparisonContext context
    )
    {
        var list1 = value1.EnumerateArray().ToArray();
        var list2 = value2.EnumerateArray().ToArray();

        var length = list1.Length;

        if (length != list2.Length)
        {
            return (ComparisonResult.Fail, context.AddDifference(length, list2.Length, "Count"));
        }

        if (length == 0)
        {
            return (ComparisonResult.Pass, context);
        }

        return Enumerable
            .Range(0, length)
            .Select(i => (value1: list1[i], value2: list2[i], index: i))
            .Aggregate(
                (result: ComparisonResult.Inconclusive, context: context),
                (acc, x) =>
                {
                    var (newResult, newContext) = Compare(
                        context.VisitingIndex(x.index),
                        x.value1,
                        x.value2
                    );
                    return (
                        acc.result.Plus(newResult),
                        acc.context.MergeDifferencesFrom(newContext)
                    );
                }
            );
    }

    private (ComparisonResult result, IComparisonContext context) CompareString(
        JsonElement value1,
        JsonElement value2,
        IComparisonContext context
    )
    {
        var str1 = value1.GetString();
        var str2 = value2.GetString();

        if (str1 != str2)
            return (ComparisonResult.Fail, context.AddDifference(str1, str2));

        return (ComparisonResult.Pass, context);
    }

    private (ComparisonResult result, IComparisonContext context) CompareNumber(
        JsonElement value1,
        JsonElement value2,
        IComparisonContext context
    )
    {
        var num1 = value1.GetDouble();
        var num2 = value2.GetDouble();

        if (num1 != num2)
            return (ComparisonResult.Fail, context.AddDifference(num1, num2));

        return (ComparisonResult.Pass, context);
    }

    private static JsonElement? AsJsonElement(object value)
    {
        if (value is JsonElement e)
            return e;

        if (value is string json)
        {
            return JsonDocument.Parse(json).RootElement;
        }

        return null;
    }
}
