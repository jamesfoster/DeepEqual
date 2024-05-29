namespace DeepEqual.Syntax;

using System.Diagnostics.Contracts;
using DeepEqual.Formatting;

public class CompareSyntax<TLeft, TRight> : IComparisonBuilder<CompareSyntax<TLeft, TRight>>
    where TLeft : notnull
    where TRight : notnull
{
    public TLeft Left { get; set; }
    public TRight Right { get; set; }

    internal IComparisonBuilder<ComparisonBuilder> Builder { get; set; }

    public CompareSyntax(TLeft left, TRight right)
    {
        Left = left;
        Right = right;
        Builder = ComparisonBuilder.Get();
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> MapProperty<A, B>(
        Expression<Func<A, object?>> left,
        Expression<Func<B, object?>> right
    )
    {
        Builder.MapProperty(left, right);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> IgnoreLeftProperty(
        Expression<Func<TLeft, object?>> property
    )
    {
        Builder.IgnoreProperty(property);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> IgnoreRightProperty(
        Expression<Func<TRight, object?>> property
    )
    {
        Builder.IgnoreProperty(property);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> IgnoreProperty<T>(Expression<Func<T, object?>> property)
    {
        Builder.IgnoreProperty(property);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> IgnoreProperty(Func<PropertyReader, bool> func)
    {
        Builder.IgnoreProperty(func);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> SkipDefault<T>()
    {
        Builder.SkipDefault<T>();
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> WithCustomComparison(IComparison comparison)
    {
        Builder.WithCustomComparison(comparison);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> WithCustomComparison(
        Func<IComparison, IComparison> comparison
    )
    {
        Builder.WithCustomComparison(comparison);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> WithCustomFormatter<TDifference>(
        IDifferenceFormatter formatter
    )
        where TDifference : Difference
    {
        Builder.WithCustomFormatter<TDifference>(formatter);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> IgnoreUnmatchedProperties()
    {
        Builder.IgnoreUnmatchedProperties();
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> ExposeInternalsOf<T>()
    {
        Builder.ExposeInternalsOf<T>();
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> ExposeInternalsOf(params Type[] types)
    {
        Builder.ExposeInternalsOf(types);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> WithFloatingPointTolerance(
        double doubleTolerance = 1e-15d,
        float singleTolerance = 1e-6f
    )
    {
        Builder.WithFloatingPointTolerance(doubleTolerance, singleTolerance);
        return this;
    }

    [Pure]
    public CompareSyntax<TLeft, TRight> IgnoreCircularReferences()
    {
        Builder.IgnoreCircularReferences();
        return this;
    }

    [Pure]
    public bool Compare()
    {
        return Left.IsDeepEqual(Right, Builder.Create());
    }

    public void Assert()
    {
        Left.ShouldDeepEqual(Right, Builder.Create(), Builder.GetFormatterFactory());
    }

    //ncrunch: no coverage start
    IComparison IComparisonBuilder<CompareSyntax<TLeft, TRight>>.Create()
    {
        throw new NotImplementedException();
    }

    IDifferenceFormatterFactory IComparisonBuilder<
        CompareSyntax<TLeft, TRight>
    >.GetFormatterFactory()
    {
        throw new NotImplementedException();
    }
    //ncrunch: no coverage end
}
