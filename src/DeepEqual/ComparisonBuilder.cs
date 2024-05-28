namespace DeepEqual;

public sealed class ComparisonBuilder : IComparisonBuilder<ComparisonBuilder>
{
	public IList<IComparison> CustomComparisons { get; set; }
	public IDictionary<Type, IDifferenceFormatter> CustomFormatters { get; set; }

	private CycleGuard CycleGuard { get; set; }
	private CompositeComparison AllComparisons { get; set; }

	public ComplexObjectComparison ComplexObjectComparison { get; set; }
	public DefaultComparison DefaultComparison { get; set; }

	public double DoubleTolerance { get; set; } = 1e-15d;
	public float SingleTolerance { get; set; } = 1e-6f;

	private static readonly Func<IComparisonBuilder<ComparisonBuilder>> DefaultGet = () => new ComparisonBuilder();
	public static Func<IComparisonBuilder<ComparisonBuilder>> Get { get; set; } = DefaultGet;
	public static void Reset() => Get = DefaultGet;


	public ComparisonBuilder()
	{
		CustomComparisons = new List<IComparison>();
		CustomFormatters = new Dictionary<Type, IDifferenceFormatter>();

		AllComparisons = new CompositeComparison();
		CycleGuard = new CycleGuard(AllComparisons);

		ComplexObjectComparison = new ComplexObjectComparison(CycleGuard);
		DefaultComparison = new DefaultComparison();
	}

	public IComparison Create()
	{
		AllComparisons.AddRange(CustomComparisons.ToArray());

		AllComparisons.AddRange([
			new FloatComparison(DoubleTolerance, SingleTolerance),
			new EnumComparison(),
			DefaultComparison,
			new DictionaryComparison(new DefaultComparison(), CycleGuard),
			new SetComparison(CycleGuard),
			new ListComparison(CycleGuard),
			ComplexObjectComparison
		]);

		return CycleGuard;
	}

	public IDifferenceFormatterFactory GetFormatterFactory()
	{
		return new DifferenceFormatterFactory(CustomFormatters);
	}

	public ComparisonBuilder IgnoreUnmatchedProperties()
	{
		ComplexObjectComparison.IgnoreUnmatchedProperties = true;

		return this;
	}

	public ComparisonBuilder WithCustomComparison(IComparison comparison)
	{
		CustomComparisons.Add(comparison);

		return this;
	}

	public ComparisonBuilder WithCustomComparison(Func<IComparison, IComparison> ctor)
	{
		CustomComparisons.Add(ctor(AllComparisons));

		return this;
	}

	public ComparisonBuilder WithCustomFormatter<TDifference>(IDifferenceFormatter formatter)
		where TDifference : Difference
	{
		CustomFormatters[typeof(TDifference)] = formatter;

		return this;
	}

	public ComparisonBuilder IgnoreProperty<T>(Expression<Func<T, object>> property)
	{
		ComplexObjectComparison.IgnoreProperty(property);

		return this;
	}

	public ComparisonBuilder IgnoreProperty(Func<PropertyReader, bool> func)
	{
		ComplexObjectComparison.IgnoreProperty(func);

		return this;
	}

	public ComparisonBuilder SkipDefault<T>()
	{
		DefaultComparison.Skip<T>();
		return this;
	}

	public ComparisonBuilder ExposeInternalsOf<T>()
	{
		return ExposeInternalsOf(typeof(T));
	}

	public ComparisonBuilder ExposeInternalsOf(params Type[] types)
	{
		ReflectionCache.CachePrivatePropertiesOfTypes(types);
		return this;
	}

	public ComparisonBuilder WithFloatingPointTolerance(double doubleTolerance = 1e-15d, float singleTolerance = 1e-6f)
	{
		DoubleTolerance = doubleTolerance;
		SingleTolerance = singleTolerance;
		return this;
	}

	public ComparisonBuilder IgnoreCircularReferences()
	{
		CycleGuard.IgnoreCircularReferences();
		return this;
	}
}