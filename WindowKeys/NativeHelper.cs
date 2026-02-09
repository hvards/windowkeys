using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WindowKeys.Interfaces;
using WindowKeys.Native;
using static WindowKeys.Native.Constants;

namespace WindowKeys;

public partial class NativeHelper(ILogger<INativeHelper> logger) : INativeHelper
{
	private LowLevelKeyboardProc? _proc;

	public nint SetKeyboardHook(LowLevelKeyboardProc lpfn)
	{
		if (_proc != null)
			throw new InvalidOperationException("Only one instance of LowLevelKeyboardProc is allowed.");

		_proc = lpfn; // To avoid garbage collection.

		using var currentProcess = Process.GetCurrentProcess();
		using var module = currentProcess.MainModule;

		return SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(module!.ModuleName), 0);
	}

	public bool UnhookKeyboardHook(IntPtr hookId)
	{
		return UnhookWindowsHookEx(hookId);
	}

	public nint CallNextHook(IntPtr hhk, int nCode, IntPtr wParam, ref KeyboardInput lParam)
	{
		return CallNextHookEx(hhk, nCode, wParam, ref lParam);
	}

	public void FocusHandle(nint windowHandle)
	{
		var blockingThread = GetWindowThreadProcessId(GetForegroundWindow(), nint.Zero);
		var ownThread = GetWindowThreadProcessId(windowHandle, nint.Zero);
		AttachThreadInput(ownThread, blockingThread, true);
		SetForegroundWindow(windowHandle);
	}

	public void ZOrderInsertWindowAfter(IntPtr windowHandle, IntPtr hWndInsertAfter)
	{
		_ = SetWindowPos(windowHandle, hWndInsertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
	}

	public bool IsWindowAtPosition(nint windowHandle, int x, int y)
	{
		var point = new Point(x, y);
		return WindowFromPoint(point) == windowHandle;
	}

	public List<Window> GetWindowsInZOrder()
	{
		var sw = Stopwatch.StartNew();
		var visibleWindows = new List<Window>();
		var nexthWnd = GetTopWindow(nint.Zero);
		var prevhWnd = nint.Zero;

		while (nexthWnd != nint.Zero)
		{
			var hWnd = nexthWnd;
			nexthWnd = GetWindow(hWnd, GW_HWNDNEXT);

			var title = GetWindowTitle(hWnd);
			if (string.IsNullOrEmpty(title)) continue;
			if (title is "Program Manager" or "Windows Input Experience" or "Default IME" or "MSCTFIME UI")
				continue;
			if (!GetWindowRectangle(hWnd, out var rect) || rect.Size < 2500) continue;
			if (visibleWindows.Any(x => Geometry.IsRectInside(x.Rect, rect))) continue;

			visibleWindows.Add(new Window { Rect = rect, Handle = hWnd, InsertAfter = prevhWnd });
			prevhWnd = hWnd;
		}

		if (sw.ElapsedMilliseconds > 100)
			LogGetWidnowsInZOrderElapsedTime(sw.ElapsedMilliseconds);

		return visibleWindows;
	}

	public void ClickKey(ushort vk, nint? action) => SendKeyboardInput(GetKeyboardInputArr(vk, action: action));

	private static void SendKeyboardInput(Input[] kbInputs) =>
		_ = SendInput((uint)kbInputs.Length, kbInputs, Marshal.SizeOf<Input>());

	private static Input[] GetKeyboardInputArr(ushort vk, ushort modifier = 0, nint? action = null) => action == null
		? modifier == 0
			? [GetKeyboardInput(vk, true), GetKeyboardInput(vk, false)]
			:
			[
				GetKeyboardInput(modifier, true), GetKeyboardInput(vk, true), GetKeyboardInput(vk, false),
				GetKeyboardInput(modifier, false)
			]
		: [GetKeyboardInput(vk, (int)action == (int)WM_KEYDOWN)];

	private static Input GetKeyboardInput(ushort vk, bool down) => new()
	{
		type = (int)InputType.Keyboard,
		u = new InputUnion
		{
			ki = new KeyboardInput { wVk = vk, dwFlags = (ushort)(down ? KeyEventF.KeyDown : KeyEventF.KeyUp) }
		}
	};

	private static bool GetWindowRectangle(nint hWnd, out RECT rectangle)
	{
		rectangle = new RECT();
		if (!IsWindowVisible(hWnd)) return false;
		if (!GetWindowRect(hWnd, out rectangle)) return false;
		_ = DwmGetWindowAttribute(hWnd, 14, out var isCloaked, Marshal.SizeOf(typeof(int)));
		return isCloaked <= 0;
	}

	private static string GetWindowTitle(nint hWnd)
	{
		var length = GetWindowTextLength(hWnd);
		var sb = new StringBuilder(length + 1);
		_ = GetWindowText(hWnd, sb, sb.Capacity);
		return sb.ToString();
	}


	[LoggerMessage(LogLevel.Information, "GetWindowsInZOrder took {milliSeconds}ms")]
	private partial void LogGetWidnowsInZOrderElapsedTime(long milliSeconds);

	#region NativeMethods

	[LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW")]
	private static partial nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool UnhookWindowsHookEx(nint hhk);

	[LibraryImport("user32.dll")]
	private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);

	[LibraryImport("user32.dll")]
	private static partial nint GetTopWindow(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetWindowRect(nint hWnd, out RECT lpRect);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
	private static partial int GetWindowTextLength(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindowVisible(nint hWnd);

	[LibraryImport("user32.dll")]
	private static partial nint GetWindow(nint hWnd, uint uCmd);

	[LibraryImport("user32.dll")]
	private static partial nint GetForegroundWindow();

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetForegroundWindow(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool AttachThreadInput(nint idAttach, nint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

	[LibraryImport("User32.dll")]
	private static partial nint GetWindowThreadProcessId(nint hwnd, nint lpdwProcessId);

	[LibraryImport("user32.dll")]
	private static partial uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
		uint uFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr WindowFromPoint(Point p);

	[LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
	private static partial nint GetModuleHandle(string lpModuleName);

	[LibraryImport("dwmapi.dll")]
	private static partial int DwmGetWindowAttribute(nint hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);
	#endregion
}