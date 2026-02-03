using WindowKeys.Interfaces;
using WindowKeys.Settings;
using WindowKeys.Native;

namespace WindowKeys.Forms;

public class OverlayForm : Form
{
	private readonly OverlaySettings _settings;
	private readonly IGeometry _geometry;
	private readonly string _activationString;
	private readonly RECT _rect;
	private readonly IReadOnlyList<RECT> _occludingRects;

	private const int BORDER_MARGIN = 16;

	public OverlayForm(RECT rect, string activationString, OverlaySettings settings, IGeometry geometry,
		IReadOnlyList<RECT> occludingRects)
	{
		_settings = settings;
		_geometry = geometry;
		_activationString = activationString;
		_rect = rect;
		_occludingRects = occludingRects;

		SetFormAppearanceAndPosition();
	}

	private void SetFormAppearanceAndPosition()
	{
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		ShowInTaskbar = false;
		TopMost = true;

		BackColor = Color.Black;
		Opacity = _settings.Opacity;
		Size = new Size(
			_rect.Right - _rect.Left - BORDER_MARGIN,
			_rect.Bottom - _rect.Top - BORDER_MARGIN
		);
		Location = new Point(_rect.Left + BORDER_MARGIN / 2, _rect.Top + BORDER_MARGIN / 2);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		DrawBorder(e.Graphics);
		DrawActivationString(e.Graphics);
	}

	private void DrawBorder(Graphics graphics)
	{
		var rectangle = new Rectangle(0, 0, Size.Width, Size.Height);
		using var pen = new Pen(ColorTranslator.FromHtml(_settings.BorderColor), _settings.BorderWidth);
		graphics.DrawRectangle(pen, rectangle);
	}

	private void DrawActivationString(Graphics graphics)
	{
		using var font = new Font(_settings.FontFamily, _settings.FontSize);
		var size = graphics.MeasureString(_activationString, font).ToSize();
		var position = _geometry.GetActivationStringPosition(_rect, _occludingRects, size);
		if (position == null) return;

		var location = new Point(
			position.Value.X - Location.X - size.Width / 2,
			position.Value.Y - Location.Y - size.Height / 2);

		var rectangle = new Rectangle(location, size);
		var textPosition = new PointF(
			rectangle.Left + (1f * rectangle.Width - size.Width) / 2,
			rectangle.Top + (1f * rectangle.Height - size.Height) / 2
		);

		using var brush = new SolidBrush(Color.White);
		graphics.DrawString(_activationString, font, brush, textPosition);
	}
}
