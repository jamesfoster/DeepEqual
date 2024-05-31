namespace DeepEqual.Syntax;

using System.Diagnostics.Contracts;
using DeepEqual.Formatting;

public static class ObjectExtensions
{
    [Pure]
    public static bool IsDeepEqual(this object actual, object expected)
    {
        return IsDeepEqual(actual, expected, null);
    }

    [Pure]
    public static bool IsDeepEqual(this object actual, object expected, IComparison? comparison)
    {
        comparison ??= ComparisonBuilder.Get().Create();

        var context = new ComparisonContext(comparison);

        var (result, _) = comparison.Compare(context, actual, expected);

        return result == ComparisonResult.Pass;
    }

    public static void ShouldDeepEqual(this object actual, object expected)
    {
        ShouldDeepEqual(actual, expected, comparison: null, formatterFactory: null);
    }

    public static void ShouldDeepEqual(this object actual, object expected, IComparison? comparison)
    {
        ShouldDeepEqual(actual, expected, comparison, formatterFactory: null);
    }

    public static void ShouldDeepEqual(
        this object actual,
        object expected,
        IComparison? comparison,
        IDifferenceFormatterFactory? formatterFactory
    )
    {
        var builder = ComparisonBuilder.Get();

        comparison ??= builder.Create();
        formatterFactory ??= builder.GetFormatterFactory();

        var context = new ComparisonContext(comparison);

        var (result, newContext) = comparison.Compare(context, actual, expected);

        if (result == ComparisonResult.Pass)
        {
            return;
        }

        var message = new DeepEqualExceptionMessageBuilder(
            newContext,
            formatterFactory
        ).GetMessage();

        throw new DeepEqualException(message, newContext);
    }

    [Pure]
    public static CompareSyntax<TActual, TExpected> WithDeepEqual<TActual, TExpected>(
        this TActual actual,
        TExpected expected
    )
        where TActual : notnull
        where TExpected : notnull
    {
        return new CompareSyntax<TActual, TExpected>(actual, expected);
    }
}
