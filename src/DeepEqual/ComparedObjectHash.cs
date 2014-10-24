using System;
using System.Collections.Generic;

namespace DeepEqual {
	public class ComparedObjectHash : IEqualityComparer<Tuple<object, object>> {
		private readonly HashSet<Tuple<object, object>> set;

		public ComparedObjectHash()
		{
			set = new HashSet<Tuple<object, object>>(this);
		}

		public bool Enabled { get; set; }

		/// <summary>
		/// Adds an object to the hash, and returns true if the object has yet to be visited.
		/// If the hash is disabled, always returns true.
		/// </summary>
		public bool Add(object item1, object item2)
		{
			if (!Enabled)
			{
				return true;
			}

			return set.Add(new Tuple<object, object>(item1, item2));
		}

		public bool Equals(Tuple<object, object> x, Tuple<object, object> y) {
			return ReferenceEquals(x.Item1, y.Item1)
				&& ReferenceEquals(x.Item2, y.Item2);
		}

		public int GetHashCode(Tuple<object, object> obj) {
			// This value may not be ideal when objects override GetHashCode and Equals, but it will still work.
			return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
		}
	}
}