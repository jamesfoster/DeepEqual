namespace DeepEqual.SystemTextJson;

public static class ComparisonBuilderExtensions
{
    public static IComparisonBuilder<T> UseSystemTextJson<T>(this IComparisonBuilder<T> builder)
        where T : IComparisonBuilder<T>
    {
        return builder
            .WithCustomComparison(new JsonDocumentComparison())
            .WithCustomComparison(new JsonElementComparison());
    }

    /// <summary>
    /// Call ComparisonBuilder.Reset to remove all global customizations
    /// </summary>
    public static void GloballyUseSystemTextJson()
    {
        var getter = ComparisonBuilder.Get;
        ComparisonBuilder.Get = () => getter().UseSystemTextJson();
    }
}
