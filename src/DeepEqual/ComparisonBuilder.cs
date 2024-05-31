namespace DeepEqual;

public sealed class ComparisonBuilder : IComparisonBuilder<ComparisonBuilder>
{
    internal IList<IComparison> CustomComparisons { get; set; }
    internal IDictionary<Type, IDifferenceFormatter> CustomFormatters { get; set; }

    internal double DoubleTolerance { get; set; } = 1e-15d;
    internal float SingleTolerance { get; set; } = 1e-6f;
    private bool ignoreUnmatchedProperties = false;
    private bool ignoreCircularReferences = false;
    private readonly List<Type> defaultSkippedTypes = [];
    private readonly List<Func<Type, Type, string, string?>> mappedProperties = [];
    private readonly List<Func<PropertyPair, bool>> ignoredProperties = [];

    private static readonly Func<IComparisonBuilder<ComparisonBuilder>> DefaultGet = () =>
        new ComparisonBuilder();
    public static Func<IComparisonBuilder<ComparisonBuilder>> Get { get; set; } = DefaultGet;

    public static void Reset() => Get = DefaultGet;

    public ComparisonBuilder()
    {
        CustomComparisons = new List<IComparison>();
        CustomFormatters = new Dictionary<Type, IDifferenceFormatter>();
    }

    public IComparison Create()
    {
        return new CycleGuard(
            ignoreCircularReferences,
            new CompositeComparison(
                [
                    .. CustomComparisons,
                    new FloatComparison(DoubleTolerance, SingleTolerance),
                    new EnumComparison(),
                    new DefaultComparison(defaultSkippedTypes),
                    new DictionaryComparison(new DefaultComparison([])),
                    new SetComparison(),
                    new ListComparison(),
                    new ComplexObjectComparison(
                        ignoreUnmatchedProperties,
                        ignoredProperties,
                        mappedProperties
                    )
                ]
            )
        );
    }

    public IDifferenceFormatterFactory GetFormatterFactory()
    {
        return new DifferenceFormatterFactory(CustomFormatters);
    }

    public ComparisonBuilder IgnoreUnmatchedProperties()
    {
        ignoreUnmatchedProperties = true;

        return this;
    }

    public ComparisonBuilder WithCustomComparison(IComparison comparison)
    {
        CustomComparisons.Add(comparison);

        return this;
    }

    public ComparisonBuilder WithCustomFormatter<TDifference>(IDifferenceFormatter formatter)
        where TDifference : Difference
    {
        CustomFormatters[typeof(TDifference)] = formatter;

        return this;
    }

    public ComparisonBuilder MapProperty<A, B>(
        Expression<Func<A, object?>> left,
        Expression<Func<B, object?>> right
    )
    {
        var leftName = GetMemberName(left);
        var rightName = GetMemberName(right);

        mappedProperties.Add(
            (leftType, rightType, propName) =>
            {
                if (leftType == typeof(A) && rightType == typeof(B) && propName == leftName)
                {
                    return rightName;
                }
                return null;
            }
        );
        return this;
    }

    public ComparisonBuilder IgnoreProperty<T>(Expression<Func<T, object?>> property)
    {
        var name = GetMemberName(property);

        IgnoreProperty(typeof(T), name);
        return this;
    }

    public ComparisonBuilder IgnorePropertyIfMissing<T>(Expression<Func<T, object?>> property)
    {
        var name = GetMemberName(property);

        IgnoreProperty(scope =>
        {
            static bool Matches(PropertyReader? reader, Type type, string name)
            {
                return reader is not null
                    && type.IsAssignableFrom(reader.DeclaringType)
                    && reader.Name == name;
            }
            static bool AssertMissing(PropertyReader? reader, string name)
            {
                return reader == null
                    ? true
                    : throw new ExpectedMissingProperty(
                        $"Expected property {name} to be missing from type {reader.DeclaringType.FullName}"
                    );
            }
            return Matches(scope.Left, typeof(T), name) && AssertMissing(scope.Right, name)
                || Matches(scope.Right, typeof(T), name) && AssertMissing(scope.Left, name);
        });

        return this;
    }

    private static string GetMemberName<T>(Expression<Func<T, object?>> property)
    {
        var exp = property.Body;

        if (exp is UnaryExpression cast)
        {
            exp = cast.Operand; // implicit cast to object
        }

        if (exp is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new ArgumentException($"Expression must be a simple member access: {property}");
    }

    private void IgnoreProperty(Type type, string? propertyName)
    {
        if (propertyName is null)
            return;

        IgnoreProperty(property =>
        {
            static bool Matches(PropertyReader? reader, Type type, string name)
            {
                return reader is not null
                    && type.IsAssignableFrom(reader.DeclaringType)
                    && reader.Name == name;
            }
            return Matches(property.Left, type, propertyName)
                || Matches(property.Right, type, propertyName);
        });
    }

    public ComparisonBuilder IgnoreProperty(Func<PropertyPair, bool> predicate)
    {
        ignoredProperties.Add(predicate);
        return this;
    }

    public ComparisonBuilder SkipDefault<T>()
    {
        defaultSkippedTypes.Add(typeof(T));
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

    public ComparisonBuilder WithFloatingPointTolerance(
        double doubleTolerance = 1e-15d,
        float singleTolerance = 1e-6f
    )
    {
        DoubleTolerance = doubleTolerance;
        SingleTolerance = singleTolerance;
        return this;
    }

    public ComparisonBuilder IgnoreCircularReferences()
    {
        ignoreCircularReferences = true;
        return this;
    }
}
