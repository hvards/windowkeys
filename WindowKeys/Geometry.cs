using WindowKeys.Interfaces;
using WindowKeys.Native;

namespace WindowKeys;

public class Geometry : IGeometry
{
	public static bool IsRectInside(RECT outer, RECT inner)
	{
		const int margin = 25;
		return inner.Left > outer.Left - margin && inner.Top > outer.Top - margin &&
			   inner.Right < outer.Right + margin && inner.Bottom < outer.Bottom + margin;
	}

	public Point? GetActivationStringPosition(RECT windowRect, IReadOnlyList<RECT> occludingRects, Size textSize)
	{
		var visibleRegions = GetVisibleRegions(windowRect, occludingRects);

		if (visibleRegions.Count == 0)
			return null;

		var rect = visibleRegions.MaxBy(r => r.Size);
		return new Point(
			(rect.Left + rect.Right) / 2,
			(rect.Top + rect.Bottom) / 2
		);
	}

	public static List<RECT> GetVisibleRegions(RECT windowRect, IReadOnlyList<RECT> occludingRects)
	{
		var regions = new List<RECT> { windowRect };

		foreach (var occluder in occludingRects)
		{
			var newRegions = new List<RECT>();
			foreach (var region in regions)
			{
				newRegions.AddRange(SubtractRectangle(region, occluder));
			}
			regions = newRegions;
		}

		return regions;
	}

	public static List<RECT> SubtractRectangle(RECT subject, RECT occluder)
	{
		if (occluder.Left >= subject.Right || occluder.Right <= subject.Left ||
			occluder.Top >= subject.Bottom || occluder.Bottom <= subject.Top)
		{
			// No intersection
			return [subject];
		}

		var clampedLeft = Math.Max(occluder.Left, subject.Left);
		var clampedRight = Math.Min(occluder.Right, subject.Right);
		var clampedTop = Math.Max(occluder.Top, subject.Top);
		var clampedBottom = Math.Min(occluder.Bottom, subject.Bottom);

		var result = new List<RECT>();

		// Left region
		if (clampedLeft > subject.Left)
		{
			result.Add(new RECT
			{
				Left = subject.Left,
				Top = subject.Top,
				Right = clampedLeft,
				Bottom = subject.Bottom
			});
		}

		// Right region
		if (clampedRight < subject.Right)
		{
			result.Add(new RECT
			{
				Left = clampedRight,
				Top = subject.Top,
				Right = subject.Right,
				Bottom = subject.Bottom
			});
		}

		// Top region
		if (clampedTop > subject.Top)
		{
			result.Add(new RECT
			{
				Left = subject.Left,
				Top = subject.Top,
				Right = subject.Right,
				Bottom = clampedTop
			});
		}

		// Bottom region
		if (clampedBottom < subject.Bottom)
		{
			result.Add(new RECT
			{
				Left = subject.Left,
				Top = clampedBottom,
				Right = subject.Right,
				Bottom = subject.Bottom
			});
		}

		return result;
	}
}