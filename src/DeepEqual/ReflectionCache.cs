namespace DeepEqual
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	using Microsoft.CSharp.RuntimeBinder;

	using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

	public static class ReflectionCache
	{
		private static readonly Dictionary<Type, Type> EnumerationTypeCache = new Dictionary<Type, Type>();
		private static readonly Dictionary<Type, bool> IsListCache = new Dictionary<Type, bool>();
		private static readonly Dictionary<Type, bool> IsSetCache = new Dictionary<Type, bool>();
		private static readonly Dictionary<Type, bool> IsDictionaryCache = new Dictionary<Type, bool>();
		private static readonly Dictionary<Type, PropertyReader[]> PropertyCache = new Dictionary<Type, PropertyReader[]>();

		public static void ClearCache()
		{
			EnumerationTypeCache.Clear();
			IsListCache.Clear();
			IsSetCache.Clear();
			IsDictionaryCache.Clear();
			PropertyCache.Clear();
		}

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

			var implements = type
				.GetInterfaces()
				.Union(new[] {type})
				.FirstOrDefault(
					t =>
						t.IsGenericType &&
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

		internal static bool IsSetType(Type type)
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

		internal static bool IsDictionaryType(Type type)
		{
			if (!IsDictionaryCache.ContainsKey(type))
			{
				var equals = type == typeof (IDictionary);
				var inherits = type.GetInterface("IDictionary") == typeof (IDictionary);

				IsDictionaryCache[type] = equals || inherits;
			}

			return IsDictionaryCache[type];
		}

		internal static bool IsValueType(Type type)
		{
			return type.IsValueType ||
			       type == typeof (string);
		}

		public static void CachePrivatePropertiesOfTypes(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				PropertyCache[type] = GetPropertiesAndFields(type, CacheBehaviour.IncludePrivate);
			}
		}

		internal static PropertyReader[] GetProperties(object obj)
		{
			var type = obj.GetType();

			if (!PropertyCache.ContainsKey(type))
			{
				if (obj is IDynamicMetaObjectProvider)
					return GetDynamicProperties(obj as IDynamicMetaObjectProvider); // Dont cache dynamic properties
				
				PropertyCache[type] = GetPropertiesAndFields(type, CacheBehaviour.PublicOnly);
			}

			return PropertyCache[type];
		}

		private static PropertyReader[] GetDynamicProperties(IDynamicMetaObjectProvider provider)
		{
			var metaObject = provider.GetMetaObject(Expression.Constant(provider));

			var memberNames = metaObject.GetDynamicMemberNames(); // may return property names as well as method names, etc.

			var result = new List<PropertyReader>();

			foreach (var name in memberNames)
			{
				try
				{
					var argumentInfo = new[] {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)};

					var binder = Binder.GetMember(CSharpBinderFlags.None, name, provider.GetType(), argumentInfo);

					var site = CallSite<Func<CallSite, object, object>>.Create(binder);

					var value = site.Target(site, provider); // will throw if no valid property getter

					result.Add(new PropertyReader
					{
						Name = name,
						DeclaringType = provider.GetType(),
						Read = o => value
					});
				}
				catch (RuntimeBinderException)
				{
				}
			}

			return result.ToArray();
		}

		private static PropertyReader[] GetPropertiesAndFields(Type type, CacheBehaviour behaviour)
		{
			return GetProperties(type, behaviour)
				.Concat(GetFields(type, behaviour))
				.ToArray();
		}

		private static IEnumerable<PropertyReader> GetProperties(Type type, CacheBehaviour behaviour)
		{
			var bindingFlags = GetBindingFlags(behaviour);
			var properties = type.GetProperties(bindingFlags).AsEnumerable();

			properties = RemoveHiddenProperties(properties);
			properties = ExcludeIndexProperties(properties);

			return properties
				.Select(
					x => new PropertyReader
						{
							Name = x.Name,
							DeclaringType = type,
							Read = o => x.GetValue(o, null)
						});
		}

		private static IEnumerable<PropertyInfo> RemoveHiddenProperties(IEnumerable<PropertyInfo> properties)
		{
			return properties
				.ToLookup(x => x.Name)
				.Select(x => x.First());
		}

		private static IEnumerable<PropertyInfo> ExcludeIndexProperties(IEnumerable<PropertyInfo> properties)
		{
			return properties
				.Where(x => !x.GetIndexParameters().Any());
		}

		private static IEnumerable<PropertyReader> GetFields(Type type, CacheBehaviour behaviour)
		{
			var bindingFlags = GetBindingFlags(behaviour);
			var fields = type.GetFields(bindingFlags).AsEnumerable();

			fields = RemoveHiddenFields(fields);

			return fields
				.Select(
					x => new PropertyReader
						{
							Name = x.Name,
							DeclaringType = type,
							Read = o => x.GetValue(o)
						});
		}

		private static IEnumerable<FieldInfo> RemoveHiddenFields(IEnumerable<FieldInfo> properties)
		{
			return properties
				.ToLookup(x => x.Name)
				.Select(x => x.First());
		}

		private static BindingFlags GetBindingFlags(CacheBehaviour behaviour)
		{
			var result = BindingFlags.Instance | BindingFlags.Public;

			if (behaviour == CacheBehaviour.IncludePrivate)
			{
				result |= BindingFlags.NonPublic;
			}

			return result;
		}
	}

	internal enum CacheBehaviour
	{
		PublicOnly,
		IncludePrivate
	}
}