using WindowKeys.Interfaces;
using WindowKeys.Native;

namespace WindowKeys;

public class Geometry(INativeHelper nativeHelper) : IGeometry
{
	public static bool IsRectInside(RECT outer, RECT inner)
	{
		const int margin = 10;
		return inner.Left > outer.Left - margin && inner.Top > outer.Top - margin &&
			   inner.Right < outer.Right + margin && inner.Bottom <= outer.Bottom + margin;
	}

	public bool TryGetActivationStringPosition(RECT rect, nint handle, Size textSize, out Point point)
	{
		point = new Point();

		var stepX = (rect.Right - rect.Left) / 4;
		var stepY = (rect.Bottom - rect.Top) / 4;

		// Get positions in this order
		// 2 6 3
		// 7 1 8
		// 4 9 5
		foreach (var (x, y) in new[]
				 { (2, 2), (1, 1), (3, 1), (1, 3), (3, 3), (1, 2), (2, 1), (2, 3), (3, 2) })
		{
			point.X = rect.Left + x * stepX;
			point.Y = rect.Top + y * stepY;
			if (TestPosition(handle, point, textSize))
				return true;
		}
		return false;
	}

	private bool TestPosition(nint handle, Point point, Size size)
	{
		var leftX = point.X - size.Width / 2;
		var rightX = point.X + size.Width / 2;
		var topY = point.Y - size.Height / 2;
		var bottomY = point.Y + size.Height / 2;

		return nativeHelper.IsWindowAtPosition(handle, leftX, topY) &&
			   nativeHelper.IsWindowAtPosition(handle, rightX, topY) &&
			   nativeHelper.IsWindowAtPosition(handle, leftX, bottomY) &&
			   nativeHelper.IsWindowAtPosition(handle, rightX, bottomY);
	}
}
