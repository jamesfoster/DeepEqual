namespace DeepEqual;

using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.CSharp.RuntimeBinder;

using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

public static class ReflectionCache
{
	private static readonly ConcurrentDictionary<Type, Type> EnumerationTypeCache = new();
	private static readonly ConcurrentDictionary<Type, bool> IsListCache = new();
	private static readonly ConcurrentDictionary<Type, bool> IsSetCache = new();
	private static readonly ConcurrentDictionary<Type, bool> IsDictionaryCache = new();
	private static readonly ConcurrentDictionary<Type, PropertyReader[]> PropertyCache = new();
	private static readonly ConcurrentDictionary<Type, bool> ValueTypeWithReferenceFieldsCache = new();

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
			return type.GetElementType()!;
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
		bool isSet(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISet<>);

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

	internal static bool IsValueTypeWithReferenceFields(Type type)
	{
		return ValueTypeWithReferenceFieldsCache.GetOrAdd(type, IsValueTypeWithReferenceFieldsImpl);
	}

	private static bool IsValueTypeWithReferenceFieldsImpl(Type type)
	{
		if (!type.IsValueType) return false;
		return type.GetProperties(GetBindingFlags(CacheBehaviour.IncludePrivate)).Any(x => x.PropertyType.IsClass);
	}

	public static void CachePrivatePropertiesOfTypes(IEnumerable<Type> types)
	{
		PropertyReader[] GetAllProperties(Type t) => GetPropertiesAndFields(t, CacheBehaviour.IncludePrivate);

		foreach (var type in types)
		{
			PropertyCache.AddOrUpdate(type, GetAllProperties, (t, value) => GetAllProperties(t));
		}
	}

	public static PropertyReader[] GetProperties(object obj)
	{
		// Dont cache dynamic properties
		if (obj is IDynamicMetaObjectProvider dyn)
		{
			return GetDynamicProperties(dyn);
		}

		var type = obj.GetType();

		PropertyReader[] getPublicProperties(Type t) => GetPropertiesAndFields(t, CacheBehaviour.PublicOnly);

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

				result.Add(new PropertyReader(name, o => value, provider.GetType()));
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
		properties = ExcludeSetOnlyProperties(properties);

		return properties
			.Select(
				x => new PropertyReader(x.Name, o => x.GetValue(o, null), type)
			);
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

	private static IEnumerable<PropertyInfo> ExcludeSetOnlyProperties(IEnumerable<PropertyInfo> properties)
	{
		return properties
			.Where(x => x.GetMethod != null);
	}

	private static IEnumerable<PropertyReader> GetFields(Type type, CacheBehaviour behaviour)
	{
		var bindingFlags = GetBindingFlags(behaviour);
		var fields = type.GetFields(bindingFlags).AsEnumerable();

		fields = RemoveHiddenFields(fields);

		return fields
			.Select(
				x => new PropertyReader(x.Name, o => x.GetValue(o), type)
			);
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