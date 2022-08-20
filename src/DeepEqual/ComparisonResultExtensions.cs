namespace DeepEqual;

using System.Collections.Generic;

public static class ComparisonResultExtensions
{
	public static ComparisonResult ToResult(this IEnumerable<ComparisonResult> results)
	{
		var foundPass = false;
		foreach (var result in results)
		{
			if (result == ComparisonResult.Fail)
				return ComparisonResult.Fail;

			if (result == ComparisonResult.Pass)
				foundPass = true;
		}

		return foundPass ? ComparisonResult.Pass : ComparisonResult.Inconclusive;
	}

	public static ComparisonResult Plus(this ComparisonResult result, ComparisonResult other)
	{
		if (result == ComparisonResult.Fail || other == ComparisonResult.Fail)
			return ComparisonResult.Fail;

		return result == ComparisonResult.Inconclusive ? other : result;
	}
}