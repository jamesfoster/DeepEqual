using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using DeepEqual.Syntax;

using Xbehave;

using Shouldly;

namespace DeepEqual.Test.Comparsions;

[SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
public class DefaultComparisonTests
{
	protected DefaultComparison SUT { get; set; }
	protected ComparisonContext Context { get; set; }

	protected ComparisonResult Result { get; set; }
	protected bool CanCompareResult { get; set; }

	[Scenario]
	public void Creating_a_DefaultComparison()
	{
		"When creating a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"Then is should implement IComparison".x(() =>
			SUT.ShouldBeAssignableTo<IComparison>()
		);
	}

	[Scenario]
	[Example(typeof (int), typeof (int))]
	[Example(typeof (string), typeof (int))]
	[Example(typeof (object), typeof (int))]
	[Example(typeof (Type), typeof (int))]
	[Example(typeof (int), typeof (object))]
	[Example(typeof (int), typeof (long))]
	public void Can_compare_any_type(Type type1, Type type2)
	{
		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"When calling CanCompare".x(() =>
			CanCompareResult = SUT.CanCompare(type1, type2)
		);

		"Then the result should be true".x(() =>
			CanCompareResult.ShouldBe(true)
		);
	}

	[Scenario]
	public void Comparing_different_types_results_in_implicit_cast()
	{
		var object1 = default (CastSpy);
		var object2 = default (string);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And 2 objects to compare".x(() =>
		{
			object1 = new CastSpy("abc");
			object2 = "abc";
		});

		"And a Comparison context object".x(() =>
			Context = new ComparisonContext()
		);

		"When calling Compare".x(() =>
			(Result, _) = SUT.Compare(Context, object1, object2)
		);

		"Then it should call the implicit operator".x(() =>
			object1.Called.ShouldBe(true)
		);

		"And it should return Pass".x(() =>
			Result.ShouldBe(ComparisonResult.Pass)
		);
	}

	[Scenario]
	public void Comparing_different_types_results_in_call_to_IConvertible()
	{
		var object1 = default (StringConvertibleSpy);
		var object2 = default (string);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And 2 objects to compare".x(() =>
		{
			object1 = new StringConvertibleSpy("abc");
			object2 = "abc";
		});

		"And a Comparison context object".x(() =>
			Context = new ComparisonContext()
		);

		"When calling Compare".x(() =>
			(Result, _) = SUT.Compare(Context, object1, object2)
		);

		"Then it should call IConvertible.ToString".x(() =>
			object1.Called.ShouldBe(true)
		);

		"And it should return Pass".x(() =>
			Result.ShouldBe(ComparisonResult.Pass)
		);
	}

	[Scenario]
	public void Comparing_incompatible_types_returns_Inconclusive()
	{
		var object1 = default (object);
		var object2 = default (int);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And 2 objects to compare".x(() =>
		{
			object1 = new object();
			object2 = 123;
		});

		"And a Comparison context object".x(() =>
			Context = new ComparisonContext()
		);

		"When calling Compare".x(() =>
			(Result, _) = SUT.Compare(Context, object1, object2)
		);

		"Then it should return Inconclusive".x(() =>
			Result.ShouldBe(ComparisonResult.Inconclusive)
		);
	}

	[Scenario]
	public void Comparing_objects_calls_Equals_method()
	{
		var object1 = default (EqualsSpy);
		var object2 = default (EqualsSpy);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And 2 objects to compare".x(() =>
		{
			object1 = new EqualsSpy();
			object2 = new EqualsSpy();
		});

		"And a Comparison context object".x(() =>
			Context = new ComparisonContext()
		);

		"When calling Compare".x(() =>
			(Result, _) = SUT.Compare(Context, object1, object2)
		);

		"Then it should call Equals".x(() =>
			object1.Calls[0].ShouldBeSameAs(object2)
		);
	}

	[Scenario]
	[Example(1, 1, ComparisonResult.Pass)]
	[Example(1, 2, ComparisonResult.Fail)]
	[Example(2, 2, ComparisonResult.Pass)]
	[Example("a", "a", ComparisonResult.Pass)]
	[Example("a", "b", ComparisonResult.Fail)]
	public void Comparing_value_types_returns_Pass_or_Fail(object value1, object value2, ComparisonResult expectedResult)
	{
		var newContext = default(IComparisonContext);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And a Comparison context object".x(() =>
			Context = new ComparisonContext("Root")
		);

		"When calling Compare".x(() =>
			(Result, newContext) = SUT.Compare(Context, value1, value2)
		);

		"The result should be Pass or Fail".x(() =>
			Result.ShouldBe(expectedResult)
		);

		if (expectedResult == ComparisonResult.Fail)
		{
			var expectedDifference = new BasicDifference(
				Breadcrumb: "Root",
				Value1: value1,
				Value2: value2,
				ChildProperty: null
			);

			"And it should add a difference".x(() =>
				newContext.Differences[0].ShouldDeepEqual(expectedDifference)
			);
		}
	}

	[Scenario]
	[Example(true)]
	[Example(false)]
	public void Comparing_object_types_returns_Pass_or_Inconclusive(bool expected)
	{
		var value1 = default (EqualsSpy);
		var value2 = default (EqualsSpy);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And 2 objects to compare".x(() =>
		{
			value1 = new EqualsSpy(expected);
			value2 = new EqualsSpy(expected);
		});

		"When calling Compare".x(() =>
			(Result, _) = SUT.Compare(null, value1, value2)
		);

		"The result should be Pass or Fail".x(() =>
			Result.ShouldBe(expected ? ComparisonResult.Pass : ComparisonResult.Inconclusive)
		);
	}

	[Scenario]
	[Example(typeof(AlwaysEqual), typeof(object))]
	[Example(typeof(object), typeof(AlwaysEqual))]
	[Example(typeof(AlwaysEqual), typeof(AlwaysEqual))]
	[Example(typeof(object), typeof(AlwaysEqualAswell))]
	[Example(typeof(AlwaysEqualAswell), typeof(object))]
	public void Calling_CanCompare_on_ignored_types_returns_false(Type type1, Type type2)
	{
		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And the type is skipped".x(() =>
			SUT.Skip<AlwaysEqual>()
		);

		"When calling Compare".x(() =>
			CanCompareResult = SUT.CanCompare(type1, type2)
		);

		"The result should be false".x(() =>
			CanCompareResult.ShouldBe(false)
		);
	}

	[Scenario]
	public void Calling_Compare_on_ignored_types_returns_Inconclusive()
	{
		var value1 = default (AlwaysEqual);
		var value2 = default (object);

		"Given a DefaultComparison".x(() =>
			SUT = new DefaultComparison()
		);

		"And the type is skipped".x(() =>
			SUT.Skip<AlwaysEqual>()
		);

		"And 2 objects to compare".x(() =>
		{
			value1 = new AlwaysEqual();
			value2 = new AlwaysEqual();
		});

		"When calling Compare".x(() =>
			(Result, _) = SUT.Compare(null, value1, value2)
		);

		"The result should be Inconclusive".x(() =>
			Result.ShouldBe(ComparisonResult.Inconclusive)
		);
	}

	private class AlwaysEqual
	{
		public override bool Equals(object obj)
		{
			return true;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}

	private class AlwaysEqualAswell : AlwaysEqual {}

	#region Spies
	//ncrunch: no coverage start

	private class EqualsSpy
	{
		private bool Result { get; }
		public List<object> Calls { get; }

		public EqualsSpy(bool result = false)
		{
			Result = result;
			Calls = new List<object>();
		}

		public override bool Equals(object obj)
		{
			Calls.Add(obj);

			return Result;
		}

		public override int GetHashCode()
		{
			return (Calls != null ? Calls.GetHashCode() : 0);
		}
	}

	private class CastSpy
	{
		private readonly string Value;
		public bool Called { get; private set; }

		public CastSpy(string value)
		{
			Value = value;
		}

		public static implicit operator string(CastSpy spy)
		{
			spy.Called = true;

			return spy.Value;
		}
	}

	private class StringConvertibleSpy : IConvertible
	{
		private readonly string Value;
		public bool Called { get; private set; }

		public StringConvertibleSpy(string value)
		{
			Value = value;
		}

		public TypeCode GetTypeCode()
		{
			return Value.GetTypeCode();
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}

		public string ToString(IFormatProvider provider)
		{
			Called = true;

			return Value;
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			throw new InvalidOperationException();
		}
	}

	//ncrunch: no coverage end
	#endregion
}