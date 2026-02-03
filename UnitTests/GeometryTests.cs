using System.Drawing;
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
		var inner = new RECT { Left = -20, Top = -20, Right = 40, Bottom = 40 };

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
		var outer = new RECT { Left = 30, Top = 30, Right = 60, Bottom = 60 };
		var inner = new RECT { Left = -10, Top = -10, Right = 30, Bottom = 30 };

		var result = Geometry.IsRectInside(outer, inner);

		Assert.That(result, Is.False);
	}

	[Test]
	public void SubtractRectangle_NoIntersection_ReturnsOriginal()
	{
		var subject = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluder = new RECT { Left = 200, Top = 200, Right = 300, Bottom = 300 };

		var result = Geometry.SubtractRectangle(subject, occluder);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Left, Is.EqualTo(0));
		Assert.That(result[0].Top, Is.EqualTo(0));
		Assert.That(result[0].Right, Is.EqualTo(100));
		Assert.That(result[0].Bottom, Is.EqualTo(100));
	}

	[Test]
	public void SubtractRectangle_CenterOcclusion_ReturnsFourPieces()
	{
		// Arrange
		var subject = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluder = new RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };

		// Act
		var result = Geometry.SubtractRectangle(subject, occluder);

		// Assert
		Assert.That(result, Has.Count.EqualTo(4));
		// Left region
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 0 && r.Right == 25 && r.Bottom == 100));
		// Right region
		Assert.That(result.Any(r => r.Left == 75 && r.Top == 0 && r.Right == 100 && r.Bottom == 100));
		// Top region
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 0 && r.Right == 100 && r.Bottom == 25));
		// Bottom region
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 75 && r.Right == 100 && r.Bottom == 100));
	}

	[Test]
	public void SubtractRectangle_TopOcclusion_ReturnsThreePieces()
	{
		// Arrange
		var subject = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluder = new RECT { Left = 25, Top = 0, Right = 75, Bottom = 50 };

		// Act
		var result = Geometry.SubtractRectangle(subject, occluder);

		// Assert
		Assert.That(result, Has.Count.EqualTo(3));

		// Left region
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 0 && r.Right == 25 && r.Bottom == 100));
		// Right region
		Assert.That(result.Any(r => r.Left == 75 && r.Top == 0 && r.Right == 100 && r.Bottom == 100));
		// Bottom region
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 50 && r.Right == 100 && r.Bottom == 100));
	}

	[Test]
	public void SubtractRectangle_FullOcclusion_ReturnsEmpty()
	{
		// Arrange
		var subject = new RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };
		var occluder = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };

		// Act
		var result = Geometry.SubtractRectangle(subject, occluder);

		// Assert
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void GetVisibleRegions_NoOccluders_ReturnsOriginal()
	{
		// Arrange
		var windowRect = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<RECT>();

		// Act
		var result = Geometry.GetVisibleRegions(windowRect, occluders);

		// Assert
		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Left, Is.EqualTo(0));
		Assert.That(result[0].Top, Is.EqualTo(0));
		Assert.That(result[0].Right, Is.EqualTo(100));
		Assert.That(result[0].Bottom, Is.EqualTo(100));
	}

	[Test]
	public void GetVisibleRegions_PartialOcclusion_ReturnsVisiblePieces()
	{
		// Arrange
		var windowRect = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<RECT>
		{
			new() { Left = 25, Top = 25, Right = 75, Bottom = 75 }
		};

		// Act
		var result = Geometry.GetVisibleRegions(windowRect, occluders);

		// Assert
		Assert.That(result, Has.Count.EqualTo(4));
		var largest = result.Max(r => r.Size);
		Assert.That(largest, Is.EqualTo(2500));
	}

	[Test]
	public void GetVisibleRegions_FullOcclusion_ReturnsEmpty()
	{
		// Arrange
		var windowRect = new RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };
		var occluders = new List<RECT>
		{
			new() { Left = 0, Top = 0, Right = 100, Bottom = 100 }
		};

		// Act
		var result = Geometry.GetVisibleRegions(windowRect, occluders);

		// Assert
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void GetVisibleRegions_MultipleOccluders_ReturnsRemainingPieces()
	{
		// Arrange
		var windowRect = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<RECT>
		{
			new() { Left = 0, Top = 0, Right = 50, Bottom = 50 },
			new() { Left = 50, Top = 50, Right = 100, Bottom = 100 }
		};

		// Act
		var result = Geometry.GetVisibleRegions(windowRect, occluders);

		// Assert
		Assert.That(result, Is.Not.Empty);
		var totalArea = result.Sum(r => r.Size);
		Assert.That(totalArea, Is.EqualTo(5000)); // Half the window area
	}

	[Test]
	public void GetActivationStringPosition_NoOcclusion_ReturnsCenterOfWindow()
	{
		// Arrange
		var geometry = new Geometry();
		var windowRect = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<RECT>();
		var textSize = new Size(10, 10);

		// Act
		var result = geometry.GetActivationStringPosition(windowRect, occluders, textSize);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Value.X, Is.EqualTo(50));
		Assert.That(result.Value.Y, Is.EqualTo(50));
	}

	[Test]
	public void GetActivationStringPosition_PartialOcclusion_ReturnsLargestVisibleArea()
	{
		// Arrange
		var geometry = new Geometry();
		var windowRect = new RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<RECT>
		{
			new() { Left = 0, Top = 0, Right = 100, Bottom = 75 }
		};
		var textSize = new Size(10, 10);

		// Act
		var result = geometry.GetActivationStringPosition(windowRect, occluders, textSize);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Value.X, Is.EqualTo(50));
		Assert.That(result.Value.Y, Is.EqualTo(87));
	}

	[Test]
	public void GetActivationStringPosition_FullOcclusion_ReturnsNull()
	{
		// Arrange
		var geometry = new Geometry();
		var windowRect = new RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };
		var occluders = new List<RECT>
		{
			new() { Left = 0, Top = 0, Right = 100, Bottom = 100 }
		};
		var textSize = new Size(10, 10);

		// Act
		var result = geometry.GetActivationStringPosition(windowRect, occluders, textSize);

		// Assert
		Assert.That(result, Is.Null);
	}
}