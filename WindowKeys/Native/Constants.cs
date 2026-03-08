namespace WindowKeys.Native;

public static class Constants
{
	internal const int WH_KEYBOARD_LL = 13;
	public const nint WM_KEYDOWN = 0x0100;
	public const nint WM_KEYUP = 0x0101;
	internal const nint WM_SYSKEYDOWN = 0x0104;
	internal const nint WM_SYSKEYUP = 0x0105;
	internal const uint GW_HWNDNEXT = 2;
	internal const uint SWP_NOSIZE = 0x0001;
	internal const uint SWP_NOMOVE = 0x0002;
}