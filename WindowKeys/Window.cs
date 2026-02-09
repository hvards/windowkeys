using WindowKeys.Forms;
using WindowKeys.Native;

namespace WindowKeys;

public class Window
{
	public nint Handle { get; set; } = nint.Zero;
	public RECT Rect { get; set; }
	public string ActivationString { get; set; } = string.Empty;
	public OverlayForm? OverlayForm { get; set; }
	public bool Dismissed { get; set; }
	public nint InsertAfter { get; set; } = nint.Zero;

	public void DismissOverlay()
	{
		OverlayForm?.Hide();
		Dismissed = true;
	}
}
