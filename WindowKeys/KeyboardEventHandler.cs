using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WindowKeys.Interfaces;
using WindowKeys.Native;
using WindowKeys.Settings;
using static WindowKeys.Native.Constants;

namespace WindowKeys;

public partial class KeyboardEventHandler(
	INativeHelper nativeHelper,
	IWindowHandler windowHandler,
	IOptions<ActivationSettings> activationSettings,
	ILogger<KeyboardEventHandler> logger)
	: IKeyboardEventHandler
{
	private readonly ActivationSettings _activationSettings = activationSettings.Value;

	public void Start()
	{
		_hookId = nativeHelper.SetKeyboardHook(KeyboardHookCallback);
	}

	private nint _hookId = nint.Zero;
	private readonly HashSet<ushort> _pressedKeys = [];
	private bool _active;
	private string _activationString = string.Empty;

	public nint KeyboardHookCallback(int nCode, nint wParam, ref KeyboardInput lParam)
	{
		var keyDown = wParam is WM_KEYDOWN or WM_SYSKEYDOWN;
		var keyUp = wParam is WM_KEYUP or WM_SYSKEYUP;
		var vkCode = lParam.wVk;

		LogKeyboardEventReceived(vkCode, keyDown);

		if (!_active)
		{
			if (keyDown) _pressedKeys.Add(vkCode);
			else _pressedKeys.Clear();

			return _activationSettings.HotKey.SetEquals(_pressedKeys)
				? Activate()
				: nativeHelper.CallNextHook(_hookId, nCode, wParam, ref lParam);
		}

		if (keyUp) return nativeHelper.CallNextHook(_hookId, nCode, wParam, ref lParam);
		_activationString += ((Keys)vkCode).ToString();

		if (!windowHandler.TestActivationString(_activationString))
			_active = false;

		return 1;
	}

	private int Activate()
	{
		nativeHelper.ClickKey(0xff, WM_KEYUP);
		windowHandler.DisplayWindows();

		_active = true;
		_activationString = string.Empty;

		return 1;
	}

	~KeyboardEventHandler()
	{
		_ = nativeHelper.UnhookKeyboardHook(_hookId);
	}

	[LoggerMessage(LogLevel.Debug, Message = "Keyboard event received: {key}, Down: {down}.")]
	private partial void LogKeyboardEventReceived(uint key, bool down);
}