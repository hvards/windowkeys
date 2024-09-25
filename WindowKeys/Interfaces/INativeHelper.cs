using WindowKeys.Native;

namespace WindowKeys.Interfaces;

public interface INativeHelper
{
	nint SetKeyboardHook(LowLevelKeyboardProc lpfn);
	bool UnhookKeyboardHook(nint hookId);
	nint CallNextHook(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);
	List<Window> GetWindowsInZOrder();
	void FocusHandle(nint handle);
	bool IsWindowAtPosition(nint windowHandle, int x, int y);
	void ZOrderInsertWindowAfter(nint windowHandle, nint hWndInsertAfter);
	void ClickKey(ushort vk, nint? action);
}