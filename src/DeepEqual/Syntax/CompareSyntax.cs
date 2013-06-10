﻿namespace DeepEqual.Syntax
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Linq.Expressions;

	public class CompareSyntax<TActual, TExpected> 
	{
		public TActual Actual { get; set; }
		public TExpected Expected { get; set; }

		protected ComparisonBuilder Builder { get; set; }

		public CompareSyntax(TActual actual, TExpected expected)
		{
			Actual = actual;
			Expected = expected;
			Builder = new ComparisonBuilder();
		}

		public CompareSyntax<TActual, TExpected> IgnoreSourceProperty(Expression<Func<TActual, object>> property)
		{
			Builder.IgnoreProperty(property);
			return this;
		}

		public CompareSyntax<TActual, TExpected> IgnoreDestinationProperty(Expression<Func<TExpected, object>> property)
		{
			Builder.IgnoreProperty(property);
			return this;
		}

		public CompareSyntax<TActual, TExpected> IgnoreProperty<T>(Expression<Func<T, object>> property)
		{
			Builder.IgnoreProperty(property);
			return this;
		}

		[Pure]
		public bool Compare()
		{
			return Actual.IsDeepEqual(Expected, Builder.Create());
		}

		public void Assert()
		{
			Actual.ShouldDeepEqual(Expected, Builder.Create());
		}
	}
}