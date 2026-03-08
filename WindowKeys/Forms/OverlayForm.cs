using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
	private readonly Font _font;
	private Size _textSize;

	public OverlayForm(OverlaySettings settings, IGeometry geometry)
	{
		_settings = settings;
		_geometry = geometry;
		_font = new Font(_settings.FontFamily, _settings.FontSize);

		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		ShowInTaskbar = false;
		TopMost = true;
		BackColor = ColorTranslator.FromHtml(_settings.BackgroundColor);
	}

	public void Configure(RECT rect, string activationString, IReadOnlyList<RECT> occludingRects)
	{
		_rect = rect;
		_activationString = activationString;
		_occludingRects = occludingRects;
		LayoutOverlay();
	}

	public void UpdateActivationString(string activationString)
	{
		_activationString = activationString;
		_textSize = TextRenderer.MeasureText(_activationString, _font, Size.Empty, TextFormatFlags.NoPadding);
		Invalidate();
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
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		DrawBorder(e.Graphics);
		DrawActivationString(e.Graphics);
	}

	private void LayoutOverlay()
	{
		_textSize = TextRenderer.MeasureText(_activationString, _font, Size.Empty, TextFormatFlags.NoPadding);
		var position = _geometry.GetActivationStringPosition(_rect, _occludingRects, _textSize);
		if (position == null) return;

		var w = _textSize.Width + 24 + _settings.BorderWidth + 1;
		var h = _textSize.Height + _settings.BorderWidth + 1;
		Size = new Size(w, h);
		Location = new Point(position.Value.X - w / 2, position.Value.Y - h / 2);

		var inset = _settings.BorderWidth / 2f;
		var radius = new SizeF(_settings.CornerRadius, _settings.CornerRadius);
		var rect = new RectangleF(inset, inset,
				ClientSize.Width - _settings.BorderWidth - 1,
				ClientSize.Height - _settings.BorderWidth - 1);

		using var path = new GraphicsPath();
		path.AddRoundedRectangle(rect, radius);
		Region = new Region(path);
	}

	private void DrawBorder(Graphics graphics)
	{
		using var pen = new Pen(ColorTranslator.FromHtml(_settings.BorderColor), _settings.BorderWidth);

		float bw = pen.Width;
		float inset = bw / 2f;

		var rect = new RectangleF(
			inset,
			inset,
			ClientSize.Width - bw - 1f,
			ClientSize.Height - bw - 1f);

		var radius = new SizeF(_settings.CornerRadius, _settings.CornerRadius);
		graphics.DrawRoundedRectangle(pen, rect, radius);
	}

	private void DrawActivationString(Graphics graphics)
	{
		var position = new Point((Width - _textSize.Width) / 2, (Height - _textSize.Height) / 2);
		TextRenderer.DrawText(graphics, _activationString, _font, position, Color.White, TextFormatFlags.NoPadding);
	}
}