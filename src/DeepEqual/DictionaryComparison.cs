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

		public ComparisonResult Compare(IComparisonContext context, object value1, object value2)
		{
			var dict1 = (IDictionary) value1;
			var dict2 = CastToDictionaryEntries((IDictionary) value2).ToDictionary(e => e.Key, e => e.Value);

			if (dict1.Count != dict2.Count)
			{
				context.AddDifference(dict1.Count, dict2.Count, "Count");
				return ComparisonResult.Fail;
			}

			if (dict1.Count == 0)
			{
				return ComparisonResult.Pass;
			}

			var results = new List<ComparisonResult>();

			foreach (DictionaryEntry entry in dict1)
			{
				var key = FindKey(dict2, entry.Key);

				if (key == null)
				{
					var difference = new MissingEntryDifference
					{
						Breadcrumb = context.Breadcrumb,
						Side = MissingSide.Expected,
						Key = entry.Key,
						Value = entry.Value
					};

					context.AddDifference(difference);

					continue;
				}

				var value = dict2[key];

				var innerContext = context.VisitingIndex(key);
				results.Add(ValueComparer.Compare(innerContext, entry.Value, value));

				dict2.Remove(key);
			}

			if(dict2.Count == 0)
				return results.ToResult();

			foreach (var entry in dict2)
			{
				var difference = new MissingEntryDifference
				{
					Breadcrumb = context.Breadcrumb,
					Side = MissingSide.Actual,
					Key = entry.Key,
					Value = entry.Value
				};

				context.AddDifference(difference);
			}

			return ComparisonResult.Fail;
		}

		private static IEnumerable<DictionaryEntry> CastToDictionaryEntries(IDictionary source)
		{
			foreach (DictionaryEntry entry in source)
			{
				yield return entry;
			}
		}

		private object FindKey(IDictionary<object, object> dictionary, object key)
		{
			var tempContext = new ComparisonContext();

			foreach (var key2 in dictionary.Keys)
			{
				if (KeyComparer.Compare(tempContext, key, key2) == ComparisonResult.Pass)
					return key2;
			}

			return null;
		}
	}
}