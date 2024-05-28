using System.Text.Json;

namespace DeepEqual.SystemTextJson;

public class JsonDocumentComparison : IComparison
{
    private static readonly Type jsonDocumentType = typeof(JsonDocument);
    private static readonly Type[] allowableTypes = { jsonDocumentType, typeof(string) };
    private static readonly JsonElementComparison jsonElementComparison = new();

    public bool CanCompare(Type type1, Type type2)
    {
        return type1 == jsonDocumentType && allowableTypes.Contains(type2)
            || type2 == jsonDocumentType && allowableTypes.Contains(type1);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object value1,
        object value2
    )
    {
        var doc1 = AsJsonDocument(value1);
        var doc2 = AsJsonDocument(value2);

        if (doc1 is null || doc2 is null)
            return (ComparisonResult.Inconclusive, context);

        return jsonElementComparison.Compare(context, doc1.RootElement, doc2.RootElement);
    }

    private static JsonDocument? AsJsonDocument(object value)
    {
        if (value is JsonDocument doc)
            return doc;

        if (value is string json)
            return JsonDocument.Parse(json);

        return null;
    }
}
