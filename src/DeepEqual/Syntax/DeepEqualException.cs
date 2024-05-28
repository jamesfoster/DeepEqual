//ncrunch: no coverage start

namespace DeepEqual.Syntax;

using System;

public class DeepEqualException : Exception
{
    public IComparisonContext Context { get; set; }

    public DeepEqualException(string message, IComparisonContext context)
        : base(message)
    {
        Context = context;
    }
}
