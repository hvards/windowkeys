using WindowKeys;
using WindowKeys.Native;

namespace UnitTests;

[TestFixture]
public class GeometryTests
{
	[Test]
	public void InnerCompletelyInsideOuter_ReturnsTrue()
	{
		var outer = new RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new RECT { Left = 20, Top = 20, Right = 40, Bottom = 40 };

		var result = Geometry.IsRectInside(outer, inner);

		Assert.That(result, Is.True);
	}

	[Test]
	public void InnerPartiallyOutsideOuter_ReturnsFalse()
	{
		var outer = new RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new RECT { Left = 0, Top = 0, Right = 40, Bottom = 40 };

		var result = Geometry.IsRectInside(outer, inner);

		Assert.That(result, Is.False);
	}

	[Test]
	public void InnerCompletelyOutsideOuter_ReturnsFalse()
	{
		var outer = new RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new RECT { Left = 60, Top = 60, Right = 100, Bottom = 100 };

		var result = Geometry.IsRectInside(outer, inner);

		Assert.That(result, Is.False);
	}

	[Test]
	public void InnerWithNegativeCoordinates_ReturnsFalse()
	{
		var outer = new RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new RECT { Left = -10, Top = -10, Right = 30, Bottom = 30 };

		var result = Geometry.IsRectInside(outer, inner);

		Assert.That(result, Is.False);
	}
}