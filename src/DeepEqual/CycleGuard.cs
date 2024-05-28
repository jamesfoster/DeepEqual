using System;
using System.Diagnostics.CodeAnalysis;

namespace DeepEqual;

public class CycleGuard(IComparison inner) : IComparison
{
    private readonly ThreadLocal<Stack<ComparisonFrame>> framesByThread = new(() => new());
    private bool ignoreCircularReferences = false;

    public IComparison Inner { get; } = inner;

    public bool CanCompare(Type type1, Type type2)
    {
        return true;
    }

    public void IgnoreCircularReferences()
    {
        ignoreCircularReferences = true;
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? value1,
        object? value2
    )
    {
        if (value1 == null || value2 == null)
        {
            return (ComparisonResult.Inconclusive, context);
        }

        var previousComparisons = PreviousComparisonsForCurrentThread();
        if (previousComparisons.Count > 0)
        {
            var arr = previousComparisons.ToArray();
            foreach (var comparison in arr)
            {
                var foundCycle =
                    ReferenceEquals(comparison.Value1, value1)
                    || ReferenceEquals(comparison.Value2, value2);

                if (foundCycle)
                {
                    return HandleCycle(comparison, context, value1, value2);
                }
            }
        }

        previousComparisons.Push(new ComparisonFrame(context.Breadcrumb, value1, value2));
        try
        {
            return Inner.Compare(context, value1, value2);
        }
        finally
        {
            previousComparisons.Pop();
        }
    }

    private (ComparisonResult result, IComparisonContext context) HandleCycle(
        ComparisonFrame frame,
        IComparisonContext context,
        object value1,
        object value2
    )
    {
        if (ignoreCircularReferences)
        {
            if (ReferenceEquals(frame.Value1, value1) && ReferenceEquals(frame.Value2, value2))
            {
                return (ComparisonResult.Pass, context);
            }
            else
            {
                return (ComparisonResult.Fail, context);
            }
        }
        else
        {
            return ThrowCircularReferenceException(frame, context, value1, value2);
        }
    }

    private static (
        ComparisonResult result,
        IComparisonContext context
    ) ThrowCircularReferenceException(
        ComparisonFrame frame,
        IComparisonContext context,
        object value1,
        object value2
    )
    {
        var message = $"""
            The traversed object graph contains a circular reference at the following location:
            ${frame.Breadcrumb}
             and
            ${context.Breadcrumb}

            If it's not possible to redesign your API to eliminate circular references
            you can change this default behavior with the following:

            {nameof(ComparisonBuilder)}.{nameof(ComparisonBuilder.IgnoreCircularReferences)}
            """;

        throw new ObjectGraphCircularReferenceException(
            message,
            context.Breadcrumb,
            value1,
            value2
        );
    }

    private Stack<ComparisonFrame> PreviousComparisonsForCurrentThread()
    {
        return framesByThread.Value!;
    }

    private readonly record struct ComparisonFrame(
        BreadcrumbPair Breadcrumb,
        object Value1,
        object Value2
    );
}
