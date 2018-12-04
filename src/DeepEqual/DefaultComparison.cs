namespace DeepEqual
{
	using System;
	using System.Collections.Generic;

	using System.Linq;

	public class DefaultComparison : IComparison
	{
		public List<Type> SkippedTypes { get; set; }

		public DefaultComparison()
		{
			SkippedTypes = new List<Type>();
		}

		public bool CanCompare(Type type1, Type type2)
		{
			return !IsSkipped(type1) && !IsSkipped(type2);
		}

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			var type1 = value1.GetType();
			var type2 = value2.GetType();

			if (IsSkipped(type1) || IsSkipped(type2))
			{
				return ComparisonResult.Inconclusive;
			}

			if (type1 != type2)
			{
				if (CanCoerceValues(ref value1, ref value2))
				{
					type1 = value1.GetType();
					type2 = value2.GetType();

					if (type1 != type2)
					{
						return ComparisonResult.Inconclusive;
					}
				}
			}

			if (value1.Equals(value2))
			{
				return ComparisonResult.Pass;
			}

			if (ReflectionCache.IsValueType(type1))
			{
				context.AddDifference(value1, value2);
				return ComparisonResult.Fail;
			}

			return ComparisonResult.Inconclusive;
		}

		private bool IsSkipped(Type type)
		{
			return SkippedTypes.Any(t => t.IsAssignableFrom(type));
		}

		public void Skip<T>()
		{
			SkippedTypes.Add(typeof (T));
		}

		private static bool CanCoerceValues(ref object value1, ref object value2)
		{
			try
			{
				value2 = Convert.ChangeType(value2, value1.GetType());
				return true;
			}
			catch{}
			
			try
			{
				value1 = Convert.ChangeType(value1, value2.GetType());
				return true;
			}
			catch{}

			return
				CallImplicitOperator(ref value2, value1.GetType()) ||
				CallImplicitOperator(ref value1, value2.GetType());
		}

		private static bool CallImplicitOperator(ref object value, Type destType)
		{
			// TODO: Use ReflectionCache

			var type = value.GetType();

			var conversionMethod = type
				.GetMethods()
				.Where(x => x.Name == "op_Implicit" || x.Name == "op_Explicit")
				.FirstOrDefault(x => x.ReturnType == destType);

			if (conversionMethod == null)
			{
				conversionMethod = destType
					.GetMethods()
					.Where(x => x.Name == "op_Implicit" || x.Name == "op_Explicit")
					.FirstOrDefault(x => x.GetParameters().First().ParameterType == type);
			}

			if (conversionMethod == null)
				return false;

			value = conversionMethod.Invoke(null, new[] {value});
			return true;
		}
	}
}