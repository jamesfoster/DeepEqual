namespace DeepEquals
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	internal static class ReflectionCache
	{
		private static readonly Dictionary<Type, Type> EnumerationTypeCache = new Dictionary<Type, Type>();
		private static readonly Dictionary<Type, bool> IsListCache = new Dictionary<Type, bool>();
		private static readonly Dictionary<Type, bool> IsSetCache = new Dictionary<Type, bool>();
		private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new Dictionary<Type, PropertyInfo[]>();

		internal static Type GetEnumerationType(Type type)
		{
			if (!EnumerationTypeCache.ContainsKey(type))
			{
				EnumerationTypeCache[type] = GetEnumerationTypeImpl(type);
			}

			return EnumerationTypeCache[type];
		}

		private static Type GetEnumerationTypeImpl(Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}

			var implements = type.GetInterfaces().Union(new[] {type}).FirstOrDefault(
				t => t.IsGenericType &&
				     t.GetGenericTypeDefinition() == typeof (IEnumerable<>)
				);

			if (implements == null)
			{
				return typeof (object);
			}

			return implements.GetGenericArguments()[0];
		}

		internal static bool IsListType(Type type)
		{
			if (!IsListCache.ContainsKey(type))
			{
				var equals = type == typeof (IEnumerable);
				var inherits = type.GetInterface("IEnumerable") == typeof (IEnumerable);

				IsListCache[type] = equals || inherits;
			}

			return IsListCache[type];
		}

		public static bool IsSetType(Type type)
		{
			if (!IsSetCache.ContainsKey(type))
			{
				Func<Type, bool> isSet = t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (ISet<>);
				var equals = isSet(type);
				var inherits = type.GetInterfaces().Any(isSet);

				IsSetCache[type] = equals || inherits;
			}

			return IsSetCache[type];
		}

		internal static bool IsValueType(Type type)
		{
			return type.IsValueType ||
			       type == typeof (string);
		}

		internal static PropertyInfo[] GetProperties(object obj)
		{
			return GetProperties(obj.GetType());
		}

		internal static PropertyInfo[] GetProperties(Type type)
		{
			if (!PropertyCache.ContainsKey(type))
			{
				PropertyCache[type] = type.GetProperties();
			}

			return PropertyCache[type];
		}
	}
}