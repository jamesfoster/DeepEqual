using System;

using DeepEqual.Syntax;

using Moq;

using Xunit;

namespace DeepEqual.Test.Syntax;

public class CompareSyntaxTests : IDisposable
{
    private readonly Left a = new Left();

    private readonly Right b = new Right();

    private readonly CompareSyntax<Left, Right> syntax;

    private readonly Mock<IComparisonBuilder<ComparisonBuilder>> builder;

    public CompareSyntaxTests()
    {
        var comparison = new CompositeComparison();

        builder = new Mock<IComparisonBuilder<ComparisonBuilder>>();

        builder
            .Setup(x => x.Create())
            .Returns(() => comparison);

        ComparisonBuilder.Get = () => builder.Object;

        syntax = a.WithDeepEqual(b);
    }

    public void Dispose()
    {
        ComparisonBuilder.Reset();
    }

    [Fact]
    public void Delegates_IgnoreUnmatchedProperties()
    {
        syntax.IgnoreUnmatchedProperties();

        builder.Verify(x => x.IgnoreUnmatchedProperties(), Times.Once());
    }

    [Fact]
    public void When_calling_IgnoreLeftProperty_ignores_the_property_on_the_left_type()
    {
        syntax.IgnoreLeftProperty(x => x.Prop);

        builder.Verify(x => x.IgnoreProperty<Left>(y => y.Prop), Times.Once());
    }

    [Fact]
    public void When_calling_IgnoreRightProperty_ignores_the_property_on_the_right_type()
    {
        syntax.IgnoreRightProperty(x => x.Prop2);

        builder.Verify(x => x.IgnoreProperty<Right>(y => y.Prop2), Times.Once());
    }

    [Fact]
    public void Delegates_MapProperty()
    {
        syntax.MapProperty<Left, Right>(x => x.Prop, x => x.Prop2);

        builder.Verify(x => x.MapProperty<Left, Right>(x => x.Prop, x => x.Prop2), Times.Once());
    }

    [Fact]
    public void Delegates_IgnoreProperty()
    {
        syntax.IgnoreProperty<Version>(x => x.Major);

        builder.Verify(x => x.IgnoreProperty<Version>(y => y.Major), Times.Once());
    }

    [Fact]
    public void Delegates_IgnorePropertyIfMissing()
    {
        syntax.IgnorePropertyIfMissing<Version>(x => x.Major);

        builder.Verify(x => x.IgnorePropertyIfMissing<Version>(y => y.Major), Times.Once());
    }

    [Fact]
    public void Delegates_IgnoreProperty_2()
    {
        Func<PropertyPair, bool> func = x => x.Left.Name == "Abc";

        syntax.IgnoreProperty(func);

        builder.Verify(x => x.IgnoreProperty(func), Times.Once());
    }

    [Fact]
    public void Delegates_SkipDefault()
    {
        syntax.SkipDefault<Version>();

        builder.Verify(x => x.SkipDefault<Version>(), Times.Once());
    }

    [Fact]
    public void Delegates_WithCustomComparison()
    {
        var c = new DefaultComparison(skippedTypes: []);

        syntax.WithCustomComparison(c);

        builder.Verify(x => x.WithCustomComparison(c), Times.Once());
    }

    [Fact]
    public void Delegates_ExposeInternalsOf_T()
    {
        syntax.ExposeInternalsOf<Version>();

        builder.Verify(x => x.ExposeInternalsOf<Version>(), Times.Once());
    }

    [Fact]
    public void Delegates_ExposeInternalsOf()
    {
        syntax.ExposeInternalsOf(typeof(Version), typeof(Uri));

        builder.Verify(x => x.ExposeInternalsOf(typeof(Version), typeof(Uri)), Times.Once());
    }

    [Fact]
    public void Delegates_WithFloatingPointTolerance()
    {
        syntax.WithFloatingPointTolerance(1, 2);

        builder.Verify(x => x.WithFloatingPointTolerance(1, 2), Times.Once());
    }

    [Fact]
    public void Calling_Compare_creates_the_comparison()
    {
        syntax.Compare();

        builder.Verify(x => x.Create(), Times.Once());
    }

    [Fact]
    public void Calling_Assert_creates_the_comparison()
    {
        try { syntax.Assert(); } catch { }

        builder.Verify(x => x.Create(), Times.Once());
    }

    private class Right
    {
        public object Prop2 { get; set; }
    }

    private class Left
    {
        public object Prop { get; set; }
    }
}