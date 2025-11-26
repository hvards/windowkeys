using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WindowKeys.Forms;
using WindowKeys.Interfaces;
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
		foreach (var win in Windows
					 .OrderBy(x => x.Rect.Left)
					 .ThenBy(x => x.Rect.Bottom)
					 .ThenBy(x => x.Rect.Right)
					 .ThenBy(x => x.Rect.Top)
				 )
		{
			win.ActivationString = combinations[cIndex++];
			win.OverlayForm = new OverlayForm(win.Rect, win.ActivationString, _overlaySettings, geometry);
			win.OverlayForm.Show();
			nativeHelper.ZOrderInsertWindowAfter(win.OverlayForm.Handle, win.InsertAfter);
		}

		if (sw.Elapsed.TotalMilliseconds > 200)
			LogDisplayWindowsElapsedTime(Windows.Count, sw.Elapsed.TotalMilliseconds);
	}

	[LoggerMessage(LogLevel.Information, "Displaying {count} windows after {milliSeconds}ms")]
	private partial void LogDisplayWindowsElapsedTime(int count, double milliSeconds);
}