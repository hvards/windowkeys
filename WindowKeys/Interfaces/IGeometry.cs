using WindowKeys.Native;

namespace WindowKeys.Interfaces;

public interface IGeometry
{
	Point? GetActivationStringPosition(RECT windowRect, IReadOnlyList<RECT> occludingRects, Size textSize);
}
