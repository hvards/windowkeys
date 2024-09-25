using Microsoft.Extensions.Options;
using WindowKeys.Interfaces;
using WindowKeys.Settings;

namespace WindowKeys;

public class CombinationGenerator(IOptions<ActivationSettings> options) : ICombinationGenerator
{
	private readonly ActivationSettings _options = options.Value;

	private char[] Letters => _options.SelectionKeys.Length > 1
		? _options.SelectionKeys
		: ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L'];

	public List<string> GetCombinations(int count)
	{
		if (count == 0) return [];
		var length = (int)Math.Ceiling(Math.Log(count) / Math.Log(Letters.Length));

		var result = new char[length];
		var results = new List<string>();
		Combinations(result, 0, results, count);
		return results;
	}

	private void Combinations(char[] current, int position, ICollection<string> results, int desiredCount)
	{
		if (results.Count == desiredCount) return;
		if (position == current.Length)
		{
			results.Add(new string(current));
			return;
		}

		foreach (var l in Letters)
		{
			current[position] = l;
			Combinations(current, position + 1, results, desiredCount);
		}
	}
}
