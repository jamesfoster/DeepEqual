namespace DeepEqual.Syntax;

using System.Diagnostics.Contracts;

using DeepEqual.Formatting;

public class CompareSyntax<TActual, TExpected> : IComparisonBuilder<CompareSyntax<TActual, TExpected>>
	where TActual : notnull
	where TExpected : notnull
{
	public TActual Actual { get; set; }
	public TExpected Expected { get; set; }

	internal IComparisonBuilder<ComparisonBuilder> Builder { get; set; }

	public CompareSyntax(TActual actual, TExpected expected)
	{
		Actual = actual;
		Expected = expected;
		Builder = ComparisonBuilder.Get();
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> IgnoreSourceProperty(Expression<Func<TActual, object>> property)
	{
		Builder.IgnoreProperty(property);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> IgnoreDestinationProperty(Expression<Func<TExpected, object>> property)
	{
		Builder.IgnoreProperty(property);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> IgnoreProperty<T>(Expression<Func<T, object>> property)
	{
		Builder.IgnoreProperty(property);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> IgnoreProperty(Func<PropertyReader, bool> func)
	{
		Builder.IgnoreProperty(func);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> SkipDefault<T>()
	{
		Builder.SkipDefault<T>();
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> WithCustomComparison(IComparison comparison)
	{
		Builder.WithCustomComparison(comparison);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> WithCustomComparison(Func<IComparison, IComparison> comparison)
	{
		Builder.WithCustomComparison(comparison);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> WithCustomFormatter<TDifference>(IDifferenceFormatter formatter)
		where TDifference : Difference
	{
		Builder.WithCustomFormatter<TDifference>(formatter);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> IgnoreUnmatchedProperties()
	{
		Builder.IgnoreUnmatchedProperties();
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> ExposeInternalsOf<T>()
	{
		Builder.ExposeInternalsOf<T>();
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> ExposeInternalsOf(params Type[] types)
	{
		Builder.ExposeInternalsOf(types);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> WithFloatingPointTolerance(double doubleTolerance = 1e-15d, float singleTolerance = 1e-6f)
	{
		Builder.WithFloatingPointTolerance(doubleTolerance, singleTolerance);
		return this;
	}

	[Pure]
	public CompareSyntax<TActual, TExpected> IgnoreCircularReferences()
	{
		Builder.IgnoreCircularReferences();
		return this;
	}

	[Pure]
	public bool Compare()
	{
		return Actual.IsDeepEqual(Expected, Builder.Create());
	}

	public void Assert()
	{
		Actual.ShouldDeepEqual(Expected, Builder.Create(), Builder.GetFormatterFactory());
	}

	//ncrunch: no coverage start
	IComparison IComparisonBuilder<CompareSyntax<TActual, TExpected>>.Create()
	{
		throw new NotImplementedException();
	}

	IDifferenceFormatterFactory IComparisonBuilder<CompareSyntax<TActual, TExpected>>.GetFormatterFactory()
	{
		throw new NotImplementedException();
	}
	//ncrunch: no coverage end
}