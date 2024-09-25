using Microsoft.Extensions.Logging.Abstractions;
using WindowKeys;
using WindowKeys.Interfaces;

namespace UnitTests;

[TestFixture]
public class NativeHelpersTests
{
	private INativeHelper _subject = null!;

	[SetUp]
	public void SetUp()
	{
		_subject = new NativeHelper(new NullLogger<INativeHelper>());
	}

	[Test, Explicit]
	public void GetVisibleWindows()
	{
		var windows = _subject.GetWindowsInZOrder();
	}
}