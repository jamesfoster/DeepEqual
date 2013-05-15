namespace DeepEquals
{
	using System.Collections.Generic;
	using System.Linq;

	public struct ComparisonResult
	{
		public static ComparisonResult Inconclusive = new ComparisonResult(0);
		public static ComparisonResult Pass = new ComparisonResult(1);
		public static ComparisonResult Fail = new ComparisonResult(2);

		private readonly int value;

		private ComparisonResult(int value)
		{
			this.value = value;
		}

		public static ComparisonResult FromResults(IEnumerable<ComparisonResult> results)
		{
			var foundPass = false;
			foreach (var result in results)
			{
				if (result == Fail)
					return Fail;

				if (result == Pass)
					foundPass = true;
			}

			return foundPass ? Pass : Inconclusive;
		}

		public static bool operator ==(ComparisonResult a, ComparisonResult b)
		{
			return a.value == b.value;
		}

		public static bool operator !=(ComparisonResult a, ComparisonResult b)
		{
			return a.value != b.value;
		}

		public bool Equals(ComparisonResult other)
		{
			return value == other.value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is ComparisonResult && Equals((ComparisonResult) obj);
		}

		public override int GetHashCode()
		{
			return value;
		}

		public override string ToString()
		{
			return this == Pass
				       ? "Pass"
				       : this == Fail
					         ? "Fail"
					         : "Inconclusive";
		}
	}
}