using DeepEqual.SystemTextJson;

using Shouldly;

using System;
using System.Text.Json;

using Xbehave;

using Xunit;

namespace DeepEqual.Test.Comparsions;

public class JsonDocumentComparisonTests
{
    protected JsonDocumentComparison SUT { get; set; }

    protected bool CanCompareResult { get; set; }

    [Scenario]
    public void Creating_a_JsonDocumentComparison()
    {
        "When creating a JsonDocumentComparison".x(() =>
            SUT = new JsonDocumentComparison()
        );

        "Then is should implement IComparison".x(() =>
            SUT.ShouldBeAssignableTo<IComparison>()
        );
    }

    [Scenario]
    [Example(typeof(JsonDocument), typeof(JsonDocument), true)]
    [Example(typeof(JsonDocument), typeof(int),          false)]
    [Example(typeof(int),          typeof(JsonDocument), false)]
    [Example(typeof(JsonDocument), typeof(string),       true)]
    [Example(typeof(string),       typeof(JsonDocument), true)]
    public void Can_compare_JSON_types(Type type1, Type type2, bool canCompare)
    {
        "Given a JsonDocumentComparison".x(() =>
            SUT = new JsonDocumentComparison()
        );

        "When calling CanCompare".x(() =>
            CanCompareResult = SUT.CanCompare(context: null!, type1, type2)
        );

        "Then the result should be {2}".x(() =>
            CanCompareResult.ShouldBe(canCompare)
        );
    }

    [Fact]
    public void Comparing_invlid_types_returns_Inconclusive()
    {
        SUT = new JsonDocumentComparison();

        var context = new ComparisonContext(SUT);

        var (result, _) = SUT.Compare(context, JsonDocument.Parse("{}"), 123);

        result.ShouldBe(ComparisonResult.Inconclusive);
    }

    [Fact]
    public void Comparing_empty_documents_returns_Pass()
    {
        SUT = new JsonDocumentComparison();

        var context = new ComparisonContext(SUT);

        var (result, _) = SUT.Compare(context, JsonDocument.Parse("{}"), JsonDocument.Parse("{}"));

        result.ShouldBe(ComparisonResult.Pass);
    }

    [Fact]
    public void Comparing_json_values_returns_Pass()
    {
        SUT = new JsonDocumentComparison();

        var context = new ComparisonContext(SUT);

        var (result, _) = SUT.Compare(context, JsonDocument.Parse("123"), JsonDocument.Parse("123"));

        result.ShouldBe(ComparisonResult.Pass);
    }

    [Theory]
    [MemberData(nameof(PositiveTestCases))]
    public void Comparing_similar_documents_returns_Pass(string json1, string json2)
    {
        var doc1 = JsonDocument.Parse(json1);
        var doc2 = JsonDocument.Parse(json2);

        SUT = new JsonDocumentComparison();

        var context = new ComparisonContext(SUT);

        var (result, _) = SUT.Compare(context, doc1, doc2);

        result.ShouldBe(ComparisonResult.Pass);
    }

    [Theory]
    [MemberData(nameof(NegativeTestCases))]
    public void Comparing_different_documents_returns_Fail(string json1, string json2)
    {
        var doc1 = JsonDocument.Parse(json1);
        var doc2 = JsonDocument.Parse(json2);

        SUT = new JsonDocumentComparison();

        var context = new ComparisonContext(SUT);

        var (result, _) = SUT.Compare(context, doc1, doc2);

        result.ShouldBe(ComparisonResult.Fail);
    }

    public static readonly object[][] NegativeTestCases = [
        [
            """{ "a": 123 }""",
            """{ "a": 234 }"""
        ],
        [
            """{ "a": [1, 2, 3] }""",
            """{ "a": [2, 3] }"""
        ],
        [
            """{ "a": true }""",
            """{ "a": false }"""
        ],
        [
            """{ "a": null }""",
            """{ "a": 10 }"""
        ],
        [
            """{ "a": "abc" }""",
            """{ "a": "def" }"""
        ],
        [
            """{ "a": "abc" }""",
            """{}"""
        ],
        [
            """{}""",
            """{ "a": "abc" }"""
        ],
        [
            """{}""",
            """[]"""
        ],
    ];

    public static readonly object[][] PositiveTestCases = [
        [
            """{ "a": 123 }""",
            """{ "a": 123 }"""
        ],
        [
            """{ "a": "abc" }""",
            """{ "a": "abc" }"""
        ],
        [
            """{ "a": [1, 2, 3] }""",
            """{ "a": [1, 2, 3] }"""
        ],
        [
            """{ "a": [] }""",
            """{ "a": [] }"""
        ],
        [
            """{}""",
            """{}"""
        ],
        [
            """[]""",
            """[]"""
        ],
        [
            """{ "a": true }""",
            """{ "a": true }"""
        ],
        [
            """{ "a": false }""",
            """{ "a": false }"""
        ],
        [
            """{ "a": null }""",
            """{ "a": null }"""
        ],
        [
            """{ "a": 123, "b": "abc", "c": [1,2,3], "d": true, "f": false, "g": null }""",
            """{ "b": "abc", "a": 123, "c": [1,2,3], "d": true, "f": false, "g": null }"""
        ],
    ];
}