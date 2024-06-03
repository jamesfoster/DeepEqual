namespace DeepEqual;

public class CycleGuard(bool ignoreCircularReferences, IComparison inner) : IComparison
{
    private readonly ThreadLocal<Stack<ComparisonFrame>> framesByThread = new(() => new());
    private readonly bool ignoreCircularReferences = ignoreCircularReferences;

    internal IComparison Inner { get; } = inner;

    public bool CanCompare(IComparisonContext context, Type leftType, Type rightType)
    {
        return true;
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        var previousComparisons = PreviousComparisonsForCurrentThread();
        if (previousComparisons.Count > 0)
        {
            var arr = previousComparisons.ToArray();
            foreach (var comparison in arr)
            {
                var foundCycle = HasFoundCycle(comparison, leftValue, rightValue);

                if (foundCycle)
                {
                    return HandleCycle(comparison, context, leftValue, rightValue);
                }
            }
        }

        previousComparisons.Push(new ComparisonFrame(context.Breadcrumb, leftValue, rightValue));
        try
        {
            return Inner.Compare(context, leftValue, rightValue);
        }
        finally
        {
            previousComparisons.Pop();
        }
    }

    private static bool HasFoundCycle(
        ComparisonFrame comparison,
        object? leftValue,
        object? rightValue
    )
    {
        return ReferenceEquals(comparison.LeftValue, leftValue)
            || ReferenceEquals(comparison.RightValue, rightValue);
    }

    private (ComparisonResult result, IComparisonContext context) HandleCycle(
        ComparisonFrame frame,
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        if (ignoreCircularReferences)
        {
            if (
                ReferenceEquals(frame.LeftValue, leftValue)
                && ReferenceEquals(frame.RightValue, rightValue)
            )
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
            return ThrowCircularReferenceException(frame, context, leftValue, rightValue);
        }
    }

    private static (
        ComparisonResult result,
        IComparisonContext context
    ) ThrowCircularReferenceException(
        ComparisonFrame frame,
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        var message = $"""
            The traversed object graph contains a circular reference at the following location:
            ${frame.Breadcrumb}
             and
            ${context.Breadcrumb}

            If it's not possible to redesign your API to eliminate circular references
            you can change this default behavior with the following:

            {nameof(ComparisonBuilder)}.{nameof(ComparisonBuilder.IgnoreCircularReferences)}()
            """;

        throw new ObjectGraphCircularReferenceException(
            message,
            context.Breadcrumb,
            leftValue,
            rightValue
        );
    }

    private Stack<ComparisonFrame> PreviousComparisonsForCurrentThread()
    {
        return framesByThread.Value!;
    }

    private readonly record struct ComparisonFrame(
        BreadcrumbPair Breadcrumb,
        object? LeftValue,
        object? RightValue
    );
}
