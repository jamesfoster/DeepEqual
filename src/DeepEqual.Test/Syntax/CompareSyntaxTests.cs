namespace DeepEqual.Test.Syntax
{
	using System;

	using DeepEqual.Syntax;
	using DeepEqual.Test.Helper;

	using Moq;

	using Shouldly;

	using Xunit;

	public class CompareSyntaxTests
	{
		private readonly A a = new A();
		private readonly B b = new B();

		private readonly CompareSyntax<A, B> syntax;
		private readonly Mock<IComparisonBuilder<ComparisonBuilder>> builder;
		private readonly CompositeComparison comparison;

		public CompareSyntaxTests()
		{
			comparison = new CompositeComparison();

			builder = new Mock<IComparisonBuilder<ComparisonBuilder>>();

			builder
				.Setup(x => x.Create())
				.Returns(() => comparison);

			syntax = a.WithDeepEqual(b);

			syntax.Builder = builder.Object;
		}

		[Fact]
		public void Delegates_IgnoreUnmatchedProperties()
		{
			syntax.IgnoreUnmatchedProperties();

			builder.Verify(x => x.IgnoreUnmatchedProperties(), Times.Once());
		}

		[Fact]
		public void When_calling_IgnoreSourceProperty_ignores_the_property_on_the_source_type()
		{
			syntax.IgnoreSourceProperty(x => x.Prop);

			builder.Verify(x => x.IgnoreProperty<A>(y => y.Prop), Times.Once());
		}

		[Fact]
		public void When_calling_IgnoreDestinationProperty_ignores_the_property_on_the_source_type()
		{
			syntax.IgnoreDestinationProperty(x => x.Prop2);

			builder.Verify(x => x.IgnoreProperty<B>(y => y.Prop2), Times.Once());
		}

		[Fact]
		public void Delegates_IgnoreProperty()
		{
			syntax.IgnoreProperty<Version>(x => x.Major);

			builder.Verify(x => x.IgnoreProperty<Version>(y => y.Major), Times.Once());
		}

		[Fact]
		public void Delegates_IgnoreProperty_2()
		{
			Func<PropertyReader, bool> func = x => x.Name == "Abc";

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
			var c = new DefaultComparison();

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
		public void Calling_Compare_creates_the_comparison()
		{
			syntax.Compare();

			builder.Verify(x => x.Create(), Times.Once());
		}

		[Fact]
		public void Calling_Assert_creates_the_comparison()
		{
			syntax.Assert();

			builder.Verify(x => x.Create(), Times.Once());
		}

		private class B
		{
			public object Prop2 { get; set; }
		}

		private class A
		{
			public object Prop { get; set; }
		}
	}
}