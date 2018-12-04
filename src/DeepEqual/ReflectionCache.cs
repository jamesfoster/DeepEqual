namespace DeepEqual
{
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
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
		private static readonly ConcurrentDictionary<Type, Type> EnumerationTypeCache
			= new ConcurrentDictionary<Type, Type>();
		private static readonly ConcurrentDictionary<Type, bool> IsListCache
			= new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, bool> IsSetCache
			= new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, bool> IsDictionaryCache
			= new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, PropertyReader[]> PropertyCache
			= new ConcurrentDictionary<Type, PropertyReader[]>();

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
			return EnumerationTypeCache.GetOrAdd(type, GetEnumerationTypeImpl);
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
			return IsListCache.GetOrAdd(type, IsListTypeImpl);
		}

		private static bool IsListTypeImpl(Type type)
		{
			var equals = type == typeof(IEnumerable);
			var inherits = type.GetInterface("IEnumerable") == typeof(IEnumerable);

			return equals || inherits;
		}

		internal static bool IsSetType(Type type)
		{
			return IsSetCache.GetOrAdd(type, IsSetTypeImpl);
		}

		private static bool IsSetTypeImpl(Type type)
		{
			Func<Type, bool> isSet = t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISet<>);
			var equals = isSet(type);
			var inherits = type.GetInterfaces().Any(isSet);

			return equals || inherits;
		}

		internal static bool IsDictionaryType(Type type)
		{
			return IsDictionaryCache.GetOrAdd(type, IsDictionaryTypeImpl);
		}

		private static bool IsDictionaryTypeImpl(Type type)
		{
			var equals = type == typeof(IDictionary);
			var inherits = type.GetInterface("IDictionary") == typeof(IDictionary);

			return equals || inherits;
		}

		internal static bool IsValueType(Type type)
		{
			return type.IsValueType ||
			       type == typeof (string);
		}

		public static void CachePrivatePropertiesOfTypes(IEnumerable<Type> types)
		{
			Func<Type, PropertyReader[]> getAllProperties =
				t => GetPropertiesAndFields(t, CacheBehaviour.IncludePrivate);

			foreach (var type in types)
			{
				PropertyCache.AddOrUpdate(type, getAllProperties, (t, value) => getAllProperties(t));
			}
		}

		public static PropertyReader[] GetProperties(object obj)
		{
			// Dont cache dynamic properties
			if (obj is IDynamicMetaObjectProvider)
			{
				return GetDynamicProperties(obj as IDynamicMetaObjectProvider);
			}

			var type = obj.GetType();

			Func<Type, PropertyReader[]> getPublicProperties =
				t => GetPropertiesAndFields(t, CacheBehaviour.PublicOnly);

			return PropertyCache.GetOrAdd(type, getPublicProperties);
		}

		private static PropertyReader[] GetDynamicProperties(IDynamicMetaObjectProvider provider)
		{
			var metaObject = provider.GetMetaObject(Expression.Constant(provider));

			// may return property names as well as method names, etc.
			var memberNames = metaObject.GetDynamicMemberNames();

			var result = new List<PropertyReader>();

			foreach (var name in memberNames)
			{
				try
				{
					var argumentInfo = new[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						};

					var binder = Binder.GetMember(
						CSharpBinderFlags.None,
						name,
						provider.GetType(),
						argumentInfo);

					var site = CallSite<Func<CallSite, object, object>>.Create(binder);

					// will throw if no valid property getter
					var value = site.Target(site, provider);

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