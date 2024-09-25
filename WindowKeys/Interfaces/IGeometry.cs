using WindowKeys.Native;

namespace WindowKeys.Interfaces;

public interface IGeometry
{
	bool TryGetActivationStringPosition(RECT rect, nint handle, Size textSize, out Point point);
}
