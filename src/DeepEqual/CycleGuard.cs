namespace DeepEqual;

public class CycleGuard(IComparison inner) : IComparison
{
    private readonly ThreadLocal<Stack<ComparisonFrame>> framesByThread = new(() => new());
    private bool ignoreCircularReferences = false;

    public IComparison Inner { get; } = inner;

    public bool CanCompare(Type leftType, Type rightType)
    {
        return true;
    }

    public void IgnoreCircularReferences()
    {
        ignoreCircularReferences = true;
    }

    public (ComparisonResult result, IComparisonContext context) Compare(
        IComparisonContext context,
        object? leftValue,
        object? rightValue
    )
    {
        if (leftValue == null || rightValue == null)
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
                    ReferenceEquals(comparison.LeftValue, leftValue)
                    || ReferenceEquals(comparison.RightValue, rightValue);

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

    private (ComparisonResult result, IComparisonContext context) HandleCycle(
        ComparisonFrame frame,
        IComparisonContext context,
        object leftValue,
        object rightValue
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
        object leftValue,
        object rightValue
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
        object LeftValue,
        object RightValue
    );
}
