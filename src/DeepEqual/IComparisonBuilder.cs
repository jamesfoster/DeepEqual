namespace DeepEqual;

/// <summary>
/// This interface exists solely to keep ComparisonBuilder and CompareSyntax in sync.
/// </summary>
public interface IComparisonBuilder<out TBuilder>
	where TBuilder : IComparisonBuilder<TBuilder>
{
	IComparison Create();
	IDifferenceFormatterFactory GetFormatterFactory();

	TBuilder IgnoreUnmatchedProperties();
	TBuilder WithCustomComparison(IComparison comparison);
	TBuilder WithCustomFormatter<TDifference>(IDifferenceFormatter formatter) where TDifference : Difference;
	TBuilder IgnoreProperty<T>(Expression<Func<T, object>> property);
	TBuilder IgnoreProperty(Func<PropertyReader, bool> func);
	TBuilder SkipDefault<T>();
	TBuilder ExposeInternalsOf<T>();
	TBuilder ExposeInternalsOf(params Type[] types);
	TBuilder WithFloatingPointTolerance(double doubleTolerance = 1e-15d, float singleTolerance = 1e-6f);
	TBuilder IgnoreCircularReferences();
}