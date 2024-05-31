using System.Text.Json;

namespace DeepEqual.SystemTextJson;

public class JsonDocumentComparison : IComparison
{
    private static readonly Type jsonDocumentType = typeof(JsonDocument);
    private static readonly Type[] allowableTypes = { jsonDocumentType, typeof(string) };
    private static readonly JsonElementComparison jsonElementComparison = new();

    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        return leftType == jsonDocumentType && allowableTypes.Contains(rightType)
            || rightType == jsonDocumentType && allowableTypes.Contains(leftType);
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        var leftDoc = AsJsonDocument(leftValue);
        var rightDoc = AsJsonDocument(rightValue);

        if (leftDoc is null || rightDoc is null)
            return (ComparisonResult.Inconclusive, context);

        return jsonElementComparison.Compare(context, leftDoc.RootElement, rightDoc.RootElement);
    }

    private static JsonDocument? AsJsonDocument(object? value)
    {
        if (value is null)
            return null;

        if (value is JsonDocument doc)
            return doc;

        if (value is string json)
            return JsonDocument.Parse(json);

        return null;
    }
}
