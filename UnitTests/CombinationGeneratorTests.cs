using Microsoft.Extensions.Options;
using WindowKeys;
using WindowKeys.Settings;

namespace UnitTests;

[TestFixture]
public class CombinationGeneratorTests
{
	[Test]
	public void GetCombinations_GeneratesExactAmountOfUniqueCombinations()
	{
		var subject = new CombinationGenerator(Options.Create(new ActivationSettings()));
		for (var i = 0; i < 1000; i++)
		{
			var combinations = subject.GetCombinations(i);
			Assert.That(combinations, Has.Count.EqualTo(i));
			Assert.That(combinations, Is.Unique);
		}
	}
}