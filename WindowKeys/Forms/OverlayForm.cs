using WindowKeys.Interfaces;
using WindowKeys.Settings;
using WindowKeys.Native;

namespace WindowKeys.Forms;

public class OverlayForm : Form
{
	private readonly OverlaySettings _settings;
	private readonly IGeometry _geometry;
	private string _activationString = string.Empty;
	private RECT _rect;
	private IReadOnlyList<RECT> _occludingRects = [];

	private const int BORDER_MARGIN = 16;

	public OverlayForm(OverlaySettings settings, IGeometry geometry)
	{
		_settings = settings;
		_geometry = geometry;

		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		ShowInTaskbar = false;
		BackColor = Color.Black;
		Opacity = _settings.Opacity;
	}

	public void Configure(RECT rect, string activationString, IReadOnlyList<RECT> occludingRects)
	{
		_rect = rect;
		_activationString = activationString;
		_occludingRects = occludingRects;

		Size = new Size(
			_rect.Right - _rect.Left - BORDER_MARGIN,
			_rect.Bottom - _rect.Top - BORDER_MARGIN
		);
		Location = new Point(_rect.Left + BORDER_MARGIN / 2, _rect.Top + BORDER_MARGIN / 2);
	}

	protected override CreateParams CreateParams
	{
		get
		{
			var cp = base.CreateParams;
			cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE 
			return cp;
		}
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

	public void UpdateActivationString(string activationString)
	{
		_activationString = activationString;
		Invalidate();
	}

	private void DrawActivationString(Graphics graphics)
	{
		if (_settings.FontSize <= 0) return;
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
