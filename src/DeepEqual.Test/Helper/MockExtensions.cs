using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Moq;

namespace DeepEqual.Test.Helper;

public static class MockExtensions
{
    public static void VerifyAll<T>(
        this IEnumerable<Mock<T>> source,
        Expression<Action<T>> action,
        Times times) where T : class
    {
        foreach (var mock in source)
        {
            mock.Verify(action, times);
        }
    }
}