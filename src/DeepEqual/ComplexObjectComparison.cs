namespace DeepEqual
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	public class ComplexObjectComparison : IComparison
	{
		public IComparison Inner { get; set; }

		public bool IgnoreUnmatchedProperties { get; set; }

		public List<Func<PropertyReader, bool>> IgnoredProperties { get; set; }

		public ComplexObjectComparison(IComparison inner)
		{
			Inner = inner;
			IgnoredProperties = new List<Func<PropertyReader, bool>>();
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return type1.IsClass && type2.IsClass;
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			if (!context.VisitObjects(value1, value2))
			{
				return ComparisonResult.Pass;
			}

			var comparer = new ComplexObjectComparer(Inner, IgnoreUnmatchedProperties, IgnoredProperties);

			return comparer.CompareObjects(context, value1, value2);
		}

		public void IgnoreProperty<T>(Expression<Func<T, object>> property)
		{
			var exp = property.Body;

			if (exp is UnaryExpression)
			{
				exp = ((UnaryExpression) exp).Operand; // implicit cast to object
			}

			var member = exp as MemberExpression;

			if (member == null)
			{
				return;
			}

			var propertyName = member.Member.Name;

			IgnoreProperty(typeof (T), propertyName);
		}

		private void IgnoreProperty(Type type, string propertyName)
		{
			IgnoreProperty(property => type.IsAssignableFrom(property.DeclaringType) && property.Name == propertyName);
		}

		public void IgnoreProperty(Func<PropertyReader, bool> item)
		{
			IgnoredProperties.Add(item);
		}
	}
}
