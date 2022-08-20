﻿using DeepEqual.Formatting;
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
			CanCompareResult = SUT.CanCompare(type1, type2)
		);

		"Then the result should be {2}".x(() =>
			CanCompareResult.ShouldBe(canCompare)
		);
	}

	[Fact]
	public void Comparing_invlid_types_returns_Inconclusive()
	{
		SUT = new JsonDocumentComparison();

		var context = new ComparisonContext();

		var (result, _) = SUT.Compare(context, JsonDocument.Parse("{}"), 123);

		result.ShouldBe(ComparisonResult.Inconclusive);
	}

	[Fact]
	public void Comparing_empty_documents_returns_Pass()
	{
		SUT = new JsonDocumentComparison();

		var context = new ComparisonContext();

		var (result, _) = SUT.Compare(context, JsonDocument.Parse("{}"), JsonDocument.Parse("{}"));

		result.ShouldBe(ComparisonResult.Pass);
	}

	[Fact]
	public void Comparing_json_values_returns_Pass()
	{
		SUT = new JsonDocumentComparison();

		var context = new ComparisonContext();

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

		var context = new ComparisonContext();

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

		var context = new ComparisonContext();

		var (result, _) = SUT.Compare(context, doc1, doc2);

		result.ShouldBe(ComparisonResult.Fail);
	}

	public static readonly object[][] NegativeTestCases = new[]
	{
		new object[] { """{ "a": 123 }""", """{ "a": 234 }""" },
		new object[] { """{ "a": [1, 2, 3] }""", """{ "a": [2, 3] }""" },
		new object[] { """{ "a": true }""", """{ "a": false }""" },
		new object[] { """{ "a": null }""", """{ "a": 10 }""" },
		new object[] { """{ "a": "abc" }""", """{ "a": "def" }""" },
		new object[] { """{ "a": "abc" }""", """{}""" },
		new object[] { """{}""", """{ "a": "abc" }""" },
	};

	public static readonly object[][] PositiveTestCases = new[]
	{
		new object[] { """{ "a": 123 }""", """{ "a": 123 }""" },
		new object[] { """{ "a": "abc" }""", """{ "a": "abc" }""" },
		new object[] { """{ "a": [1, 2, 3] }""", """{ "a": [1, 2, 3] }""" },
		new object[] { """{ "a": [] }""", """{ "a": [] }""" },
		new object[] { """{}""", """{}""" },
		new object[] { """[]""", """[]""" },
		new object[] { """{ "a": true }""", """{ "a": true }""" },
		new object[] { """{ "a": false }""", """{ "a": false }""" },
		new object[] { """{ "a": null }""", """{ "a": null }""" },
		new object[]
        {
			"""{ "a": 123, "b": "abc", "c": [1,2,3], "d": true, "f": false, "g": null }""",
			"""{ "b": "abc", "a": 123, "c": [1,2,3], "d": true, "f": false, "g": null }"""
		},
	};
}