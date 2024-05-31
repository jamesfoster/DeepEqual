﻿namespace DeepEqual;

public interface IComparisonContext
{
    ImmutableList<Difference> Differences { get; }
    BreadcrumbPair Breadcrumb { get; }

    IComparisonContext AddDifference(Difference difference);
    IComparisonContext SetBreadcrumb(BreadcrumbPair breadcrumb);

    IComparisonContext NewEmptyContext();

    bool CanCompare(Type left, Type right);
    (ComparisonResult result, IComparisonContext context) Compare(object? left, object? right);
}
