using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WindowKeys.Forms;
using WindowKeys.Interfaces;
using WindowKeys.Native;
using WindowKeys.Settings;

namespace WindowKeys;

public partial class WindowHandler(
	INativeHelper nativeHelper,
	ICombinationGenerator combinationGenerator,
	IGeometry geometry,
	ILogger<WindowHandler> logger,
	IOptions<OverlaySettings> overlaySettings
	) : IWindowHandler
{
	private readonly OverlaySettings _overlaySettings = overlaySettings.Value;
	private readonly List<OverlayForm> _overlayForms = [];
	private List<Window> Windows { get; set; } = [];

	public bool TestActivationString(string input)
	{
		var match = false;
		foreach (var window in Windows.Where(x => !x.Dismissed))
		{
			if (window.ActivationString.StartsWith(input))
			{
				if (window.ActivationString == input)
				{
					foreach (var win in Windows.Where(x => !x.Dismissed))
						win.DismissOverlay();
					nativeHelper.FocusHandle(window.Handle);
					return false;
				}
				window.OverlayForm?.UpdateActivationString(window.ActivationString[input.Length..]);
				match = true;
				continue;
			}
			window.DismissOverlay();
		}

		return match;
	}

	public void DisplayWindows()
	{
		Windows = nativeHelper.GetWindowsInZOrder();

		var sw = Stopwatch.StartNew();

		var combinations = combinationGenerator.GetCombinations(Windows.Count);
		var cIndex = 0;
		var formIndex = 0;

		// Add new forms if required
		while (_overlayForms.Count < Windows.Count)
			_overlayForms.Add(new OverlayForm(_overlaySettings, geometry));

		foreach (var win in Windows
					 .OrderBy(x => x.Rect.Left)
					 .ThenBy(x => x.Rect.Bottom)
					 .ThenBy(x => x.Rect.Right)
					 .ThenBy(x => x.Rect.Top)
				 )
		{
			win.ActivationString = combinations[cIndex++];

			var occludingRects = GetOccludingRects(win, Windows);

			var form = _overlayForms[formIndex++];
			form.Configure(win.Rect, win.ActivationString, occludingRects);
			win.OverlayForm = form;
			form.Show();
			nativeHelper.ZOrderInsertWindowAfter(form.Handle, win.InsertAfter);
		}

		if (sw.Elapsed.TotalMilliseconds > 200)
			LogDisplayWindowsElapsedTime(Windows.Count, sw.Elapsed.TotalMilliseconds);
	}

	private static List<RECT> GetOccludingRects(Window targetWindow, List<Window> allWindows)
	{
		var occludingRects = new List<RECT>();

		// Windows list in Z-order
		foreach (var win in allWindows)
		{
			if (win == targetWindow)
				break; // Subsequent windows below target

			if (win.Rect.Left > targetWindow.Rect.Right)
				continue;
			if (win.Rect.Right < targetWindow.Rect.Left)
				continue;
			if (win.Rect.Top > targetWindow.Rect.Bottom)
				continue;
			if (win.Rect.Bottom < targetWindow.Rect.Top)
				continue;

			occludingRects.Add(win.Rect);
		}

		return occludingRects;
	}

	[LoggerMessage(LogLevel.Information, "Displaying {count} windows after {milliSeconds}ms")]
	private partial void LogDisplayWindowsElapsedTime(int count, double milliSeconds);
}
