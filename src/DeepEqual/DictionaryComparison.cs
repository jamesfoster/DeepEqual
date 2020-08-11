namespace DeepEqual
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

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

		public (ComparisonResult result, IComparisonContext context) Compare(IComparisonContext context, object value1, object value2)
		{
			var dict1 = (IDictionary) value1;
			var dict2 = CastToDictionaryEntries((IDictionary) value2).ToDictionary(e => e.Key, e => e.Value);

			if (dict1.Count != dict2.Count)
			{
				return (ComparisonResult.Fail, context.AddDifference(dict1.Count, dict2.Count, "Count"));
			}

			if (dict1.Count == 0)
			{
				return (ComparisonResult.Pass, context);
			}

			var results = new List<ComparisonResult>();

			foreach (DictionaryEntry entry in dict1)
			{
				var key = FindKey(dict2, entry.Key);

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

				var value = dict2[key];
				dict2.Remove(key);

				var (result, innerContext) = ValueComparer.Compare(new ComparisonContext($"{(object) context.Breadcrumb}[{key}]"), entry.Value, value);

				results.Add(result);
				context = context.MergeDifferencesFrom(innerContext);
			}

			if(dict2.Count == 0)
				return (results.ToResult(), context);

			foreach (var entry in dict2)
			{
				var difference = new MissingEntryDifference(
					context.Breadcrumb,
					MissingSide.Actual,
					entry.Key,
					entry.Value
				);

				context = context.AddDifference(difference);
			}

			return (ComparisonResult.Fail, context);
		}

		private static IEnumerable<DictionaryEntry> CastToDictionaryEntries(IDictionary source)
		{
			foreach (DictionaryEntry entry in source)
				yield return entry;
		}

		private object FindKey(IDictionary<object, object> dictionary, object key)
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
}
