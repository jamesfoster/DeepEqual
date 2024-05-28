namespace DeepEqual;

public class DictionaryComparison : IComparison
{
	public IComparison KeyComparer { get; set; }
	public IComparison ValueComparer { get; set; }

	public DictionaryComparison(IComparison keyComparer, IComparison valueComparer)
	{
		KeyComparer = keyComparer;
		ValueComparer = valueComparer;
	}

	public bool CanCompare(Type type1, Type type2)
	{
		return ReflectionCache.IsDictionaryType(type1) && ReflectionCache.IsDictionaryType(type2);
	}

	public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object? value1, object? value2)
	{
		if (value1 is not IDictionary dict1)
		{
			return (ComparisonResult.Inconclusive, context);
		}
		if (value2 is not IDictionary dict2)
		{
			return (ComparisonResult.Inconclusive, context);
		}

		if (dict1.Count != dict2.Count)
		{
			return (ComparisonResult.Fail, context.AddDifference(dict1.Count, dict2.Count, "Count"));
		}

		var dict2entries = CastToDictionaryEntries(dict2).ToDictionary(e => e.Key, e => (object?)e.Value);

		if (dict1.Count == 0)
		{
			return (ComparisonResult.Pass, context);
		}

		var newContext = context;
		var results = new List<ComparisonResult>();

		foreach (DictionaryEntry entry in dict1)
		{
			var key = FindKey(dict2entries, entry.Key);

			if (key == null)
			{
				var difference = new MissingEntryDifference(
					context.Breadcrumb,
					MissingSide.Expected,
					entry.Key,
					entry.Value
				);

				context.AddDifference(difference);

				continue;
			}

			var value = dict2entries[key];
			dict2entries.Remove(key);

			var (result, innerContext) = ValueComparer.Compare(context.VisitingIndex(key), entry.Value, value);

			results.Add(result);
			newContext = newContext.MergeDifferencesFrom(innerContext);
		}

		if(dict2entries.Count == 0)
			return (results.ToResult(), newContext);

		foreach (var entry in dict2entries)
		{
			var difference = new MissingEntryDifference(
				context.Breadcrumb,
				MissingSide.Actual,
				entry.Key,
				entry.Value
			);

			newContext = newContext.AddDifference(difference);
		}

		return (ComparisonResult.Fail, newContext);
	}

	private static IEnumerable<DictionaryEntry> CastToDictionaryEntries(IDictionary source)
	{
		foreach (DictionaryEntry entry in source)
			yield return entry;
	}

	private object? FindKey(IDictionary<object, object?> dictionary, object key)
	{
		var tempContext = new ComparisonContext();

		foreach (var key2 in dictionary.Keys)
		{
			var (result, _) = KeyComparer.Compare(tempContext, key, key2);
			if (result == ComparisonResult.Pass)
				return key2;
		}

		return null;
	}
}