using DeepEqual.SystemTextJson;

using Shouldly;

using System;
using System.Text.Json;

using Xbehave;

using Xunit;

namespace DeepEqual.Test.Comparsions;

public class JsonElementComparisonTests
{
    protected JsonElementComparison SUT { get; set; }

    protected bool CanCompareResult { get; set; }

    [Scenario]
    public void Creating_a_JsonElementComparison()
    {
        "When creating a JsonElementComparison".x(() =>
            SUT = new JsonElementComparison()
        );

        "Then is should implement IComparison".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    [Scenario]
    [Example(typeof(JsonElement), typeof(JsonElement), true)]
    [Example(typeof(JsonElement), typeof(int), false)]
    [Example(typeof(int), typeof(JsonElement), false)]
    [Example(typeof(JsonElement), typeof(string), true)]
    [Example(typeof(string), typeof(JsonElement), true)]
    public void Can_compare_JSON_types(Type type1, Type type2, bool canCompare)
    {
        "Given a JsonElementComparison".x(() =>
            SUT = new JsonElementComparison()
        );

        "When calling CanCompare".x(() =>
            CanCompareResult = SUT.CanCompare(type1, type2)
        );

        "Then the result should be {2}".x(() =>
            CanCompareResult.ShouldBe(canCompare)
        );
    }

    [Fact]
    public void Comparing_invlid_types_returns_Inconclusive()
    {
        SUT = new JsonElementComparison();

        var context = new ComparisonContext();

        var (result, _) = SUT.Compare(context, ParseElement("{}"), 123);

        result.ShouldBe(ComparisonResult.Inconclusive);
    }

    [Fact]
    public void Comparing_empty_documents_returns_Pass()
    {
        SUT = new JsonElementComparison();

        var context = new ComparisonContext();

        var (result, _) = SUT.Compare(context, ParseElement("{}"), ParseElement("{}"));

        result.ShouldBe(ComparisonResult.Pass);
    }

    [Fact]
    public void Comparing_json_values_returns_Pass()
    {
        SUT = new JsonElementComparison();

        var context = new ComparisonContext();

        var (result, _) = SUT.Compare(context, ParseElement("123"), ParseElement("123"));

        result.ShouldBe(ComparisonResult.Pass);
    }

    [Theory]
    [MemberData(nameof(PositiveTestCases))]
    public void Comparing_similar_documents_returns_Pass(string json1, string json2)
    {
        var doc1 = ParseElement(json1);
        var doc2 = ParseElement(json2);

        SUT = new JsonElementComparison();

        var context = new ComparisonContext();

        var (result, _) = SUT.Compare(context, doc1, doc2);

        result.ShouldBe(ComparisonResult.Pass);
    }

    [Theory]
    [MemberData(nameof(NegativeTestCases))]
    public void Comparing_different_documents_returns_Fail(string json1, string json2)
    {
        var doc1 = ParseElement(json1);
        var doc2 = ParseElement(json2);

        SUT = new JsonElementComparison();

        var context = new ComparisonContext();

        var (result, _) = SUT.Compare(context, doc1, doc2);

        result.ShouldBe(ComparisonResult.Fail);
    }

    private JsonElement ParseElement(string str) => JsonDocument.Parse(str).RootElement;

    public static readonly object[][] NegativeTestCases = JsonDocumentComparisonTests.NegativeTestCases;
    public static readonly object[][] PositiveTestCases = JsonDocumentComparisonTests.PositiveTestCases;
}
