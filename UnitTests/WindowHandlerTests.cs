using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WindowKeys;
using WindowKeys.Interfaces;
using WindowKeys.Settings;

namespace UnitTests;

public class WindowHandlerTests
{
	private WindowHandler _subject;
	private Mock<INativeHelper> _nativeHelper;
	private Mock<ICombinationGenerator> _combinationGenerator;
	private Mock<IGeometry> _geometry;

	[SetUp]
	public void SetUp()
	{
		_nativeHelper = new Mock<INativeHelper>();
		_combinationGenerator = new Mock<ICombinationGenerator>();
		_geometry = new Mock<IGeometry>();
		var settings = new OverlaySettings();

		_subject = new WindowHandler(_nativeHelper.Object, _combinationGenerator.Object, _geometry.Object,
			new NullLogger<WindowHandler>(), Options.Create(settings));
	}

	[Test]
	public void DisplayWindows_ShouldSetZOrder()
	{
		var window = new Window { InsertAfter = 456 };
		_nativeHelper.Setup(x => x.GetWindowsInZOrder()).Returns([window]);
		_combinationGenerator.Setup(x => x.GetCombinations(1)).Returns(["A"]);

		_subject.DisplayWindows();

		_nativeHelper.Verify(x => x.ZOrderInsertWindowAfter(It.IsAny<nint>(), window.InsertAfter), Times.Once);
	}

	[Test]
	public void TestActivationString_IfNoMatches_ShouldReturnFalse()
	{
		var window = new Window();
		_nativeHelper.Setup(x => x.GetWindowsInZOrder()).Returns([window]);
		_combinationGenerator.Setup(x => x.GetCombinations(1)).Returns(["A"]);

		_subject.DisplayWindows();

		var res = _subject.TestActivationString("B");

		Assert.That(res, Is.False);
	}

	[Test]
	public void TestActivationString_IfMatch_ShouldReturnFalseAndFocusHandle()
	{
		var window = new Window { Handle = 123 };
		_nativeHelper.Setup(x => x.GetWindowsInZOrder()).Returns([window]);
		_combinationGenerator.Setup(x => x.GetCombinations(1)).Returns(["A"]);

		_subject.DisplayWindows();

		var res = _subject.TestActivationString("A");

		Assert.That(res, Is.False);
		_nativeHelper.Verify(x => x.FocusHandle(window.Handle), Times.Once);
	}

	[Test]
	public void TestActivationString_IfPartialMatch_ShouldReturnTrue()
	{
		var window = new Window();
		_nativeHelper.Setup(x => x.GetWindowsInZOrder()).Returns([window]);
		_combinationGenerator.Setup(x => x.GetCombinations(1)).Returns(["AB"]);

		_subject.DisplayWindows();

		var res = _subject.TestActivationString("A");

		Assert.That(res, Is.True);
	}
}
