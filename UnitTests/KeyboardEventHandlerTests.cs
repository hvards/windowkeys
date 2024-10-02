using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WindowKeys;
using WindowKeys.Interfaces;
using WindowKeys.Native;
using WindowKeys.Settings;
using static WindowKeys.Native.Constants;

namespace UnitTests;

public class KeyboardEventHandlerTests
{
	private Mock<INativeHelper> _nativeHelper = new();
	private Mock<IWindowHandler> _windowHandler = new();
	private ActivationSettings _settings = new();

	private KeyboardEventHandler _subject;

	[SetUp]
	public void SetUp()
	{
		_nativeHelper = new Mock<INativeHelper>();
		_windowHandler = new Mock<IWindowHandler>();

		_settings = new ActivationSettings { HotKey = [90] };

		_subject = new KeyboardEventHandler(_nativeHelper.Object, _windowHandler.Object, Options.Create(_settings),
			new NullLogger<KeyboardEventHandler>());
	}

	[Test]
	public void Start_ShouldSetKeyboardHook()
	{
		_subject.Start();

		_nativeHelper.Verify(x => x.SetKeyboardHook(It.IsAny<LowLevelKeyboardProc>()), Times.Once);
	}

	[Test]
	public void KeyboardHook_ShouldDisplayOverlays_IfActivated()
	{
		// activate
		var keyboardInput = new KeyboardInput
		{
			wVk = 90
		};
		_subject.KeyboardHookCallback(0, WM_KEYDOWN, ref keyboardInput);

		_windowHandler.Verify(x => x.DisplayWindows(), Times.Once);
	}

	[Test]
	public void KeyboardHook_ShouldSendRandomKeyUp_IfActivated()
	{
		// activate
		var keyboardInput = new KeyboardInput
		{
			wVk = 90
		};
		_subject.KeyboardHookCallback(0, WM_KEYDOWN, ref keyboardInput);

		_nativeHelper.Verify(x => x.ClickKey(0xff, WM_KEYUP));
	}

	[Test]
	public void KeyboardHook_KeyDownShouldReturn1_IfActivated()
	{
		// activate
		var keyboardInput = new KeyboardInput
		{
			wVk = 90
		};
		_subject.KeyboardHookCallback(0, WM_KEYDOWN, ref keyboardInput);

		// test key blocked
		keyboardInput.wVk = 80;
		var result = _subject.KeyboardHookCallback(0, WM_KEYDOWN, ref keyboardInput);
		Assert.That(result, Is.EqualTo((nint)1));
	}

	[Test]
	public void KeyboardHook_KeyUpShouldCallNextHook_IfActivated()
	{
		// activate
		var keyboardInput = new KeyboardInput
		{
			wVk = 90
		};
		_subject.KeyboardHookCallback(0, WM_KEYDOWN, ref keyboardInput);

		// test key not blocked
		keyboardInput.wVk = 80;
		var result = _subject.KeyboardHookCallback(0, WM_KEYUP, ref keyboardInput);
		Assert.That(result, Is.EqualTo(nint.Zero));
		_nativeHelper.Verify(x => x.CallNextHook(0, 0, WM_KEYUP, ref keyboardInput), Times.Once);
	}
}