using System.Drawing.Drawing2D;
using Cryptography_App.Helpers;

namespace Cryptography_App.Controls
{
    /// <summary>
    /// Modern Icon Button - button with real icon image
    /// </summary>
    public class IconButton : Button
    {
        private Color _hoverColor = Color.FromArgb(70, 130, 180);
        private Color _pressColor = Color.FromArgb(50, 100, 150);
        private Color _normalColor = Color.FromArgb(52, 152, 219);
        private bool _isHovering = false;
#pragma warning disable CS0414
        private bool _isPressed = false;
#pragma warning restore CS0414
        private int _borderRadius = 12;
        private Color _currentColor;
        private System.Windows.Forms.Timer? _animTimer;
        private int _animStep = 0;
        private Image? _icon = null;
        private IconManager.IconType? _iconType = null;
        private int _iconSize = 20;
        private bool _iconOnLeft = true;
        private int _iconPadding = 8;

        public Image? Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        public IconManager.IconType? IconType
        {
            get => _iconType;
            set
            {
                _iconType = value;
                if (value.HasValue)
                    _icon = IconManager.GetIcon(value.Value, _iconSize);
                Invalidate();
            }
        }

        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = value;
                if (_iconType.HasValue)
                    _icon = IconManager.GetIcon(_iconType.Value, _iconSize);
                Invalidate();
            }
        }

        public bool IconOnLeft
        {
            get => _iconOnLeft;
            set { _iconOnLeft = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get => _hoverColor;
            set => _hoverColor = value;
        }

        public Color PressColor
        {
            get => _pressColor;
            set => _pressColor = value;
        }

        public Color NormalColor
        {
            get => _normalColor;
            set { _normalColor = value; _currentColor = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; UpdateRegion(); Invalidate(); }
        }

        public IconButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            _currentColor = _normalColor;
            BackColor = _normalColor;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(140, 42);
        }

        private void UpdateRegion()
        {
            using var path = GetRoundedRect(new Rectangle(0, 0, Width, Height), _borderRadius);
            Region = new Region(path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;
            AnimateColor(_hoverColor);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            _isPressed = false;
            AnimateColor(_normalColor);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            _currentColor = _pressColor;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            _currentColor = _isHovering ? _hoverColor : _normalColor;
            Invalidate();
        }

        private void AnimateColor(Color targetColor)
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();
            
            _animStep = 0;
            Color startColor = _currentColor;
            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (s, e) =>
            {
                _animStep++;
                float t = Math.Min(1f, _animStep / 10f);
                _currentColor = BlendColors(startColor, targetColor, t);
                Invalidate();
                if (t >= 1f)
                {
                    _animTimer?.Stop();
                    _animTimer?.Dispose();
                }
            };
            _animTimer.Start();
        }

        private Color BlendColors(Color from, Color to, float t)
        {
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(Parent?.BackColor ?? Color.Transparent);

            using var path = GetRoundedRect(ClientRectangle, _borderRadius);
            
            // Draw shadow
            using (var shadowPath = GetRoundedRect(new Rectangle(2, 2, Width - 2, Height - 2), _borderRadius))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadowPath);
            }

            // Draw button background with gradient
            using (var brush = new LinearGradientBrush(ClientRectangle, 
                ControlPaint.Light(_currentColor, 0.1f), _currentColor, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // Calculate content layout
            var textSize = g.MeasureString(Text, Font);
            int totalWidth = (int)textSize.Width;
            if (_icon != null)
                totalWidth += _iconSize + _iconPadding;

            int startX = (Width - totalWidth) / 2;
            int iconY = (Height - _iconSize) / 2;
            int textY = (Height - (int)textSize.Height) / 2;

            // Draw icon and text
            if (_icon != null)
            {
                if (_iconOnLeft)
                {
                    g.DrawImage(_icon, startX, iconY, _iconSize, _iconSize);
                    using var textBrush = new SolidBrush(ForeColor);
                    g.DrawString(Text, Font, textBrush, startX + _iconSize + _iconPadding, textY);
                }
                else
                {
                    using var textBrush = new SolidBrush(ForeColor);
                    g.DrawString(Text, Font, textBrush, startX, textY);
                    g.DrawImage(_icon, startX + (int)textSize.Width + _iconPadding, iconY, _iconSize, _iconSize);
                }
            }
            else
            {
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(ForeColor);
                g.DrawString(Text, Font, textBrush, ClientRectangle, sf);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Modern rounded button with hover effects and smooth animations
    /// </summary>
    public class ModernButton : Button
    {
        private Color _hoverColor = Color.FromArgb(70, 130, 180);
        private Color _pressColor = Color.FromArgb(50, 100, 150);
        private Color _normalColor = Color.FromArgb(52, 152, 219);
        private bool _isHovering = false;
        #pragma warning disable CS0414
        private bool _isPressed = false;
        #pragma warning restore CS0414
        private int _borderRadius = 12;
        private Color _currentColor;
        private System.Windows.Forms.Timer? _animTimer;
        private int _animStep = 0;

        public Color HoverColor
        {
            get => _hoverColor;
            set => _hoverColor = value;
        }

        public Color PressColor
        {
            get => _pressColor;
            set => _pressColor = value;
        }

        public Color NormalColor
        {
            get => _normalColor;
            set { _normalColor = value; _currentColor = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; UpdateRegion(); Invalidate(); }
        }

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            _currentColor = _normalColor;
            BackColor = _normalColor;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(120, 40);
        }

        private void UpdateRegion()
        {
            using var path = GetRoundedRect(new Rectangle(0, 0, Width, Height), _borderRadius);
            Region = new Region(path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;
            AnimateColor(_hoverColor);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            _isPressed = false;
            AnimateColor(_normalColor);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            _currentColor = _pressColor;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            _currentColor = _isHovering ? _hoverColor : _normalColor;
            Invalidate();
        }

        private void AnimateColor(Color targetColor)
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();
            
            _animStep = 0;
            Color startColor = _currentColor;
            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (s, e) =>
            {
                _animStep++;
                float t = Math.Min(1f, _animStep / 10f);
                _currentColor = BlendColors(startColor, targetColor, t);
                Invalidate();
                if (t >= 1f)
                {
                    _animTimer?.Stop();
                    _animTimer?.Dispose();
                }
            };
            _animTimer.Start();
        }

        private Color BlendColors(Color from, Color to, float t)
        {
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.Transparent);

            using var path = GetRoundedRect(ClientRectangle, _borderRadius);
            
            // Draw shadow
            using (var shadowPath = GetRoundedRect(new Rectangle(2, 2, Width - 2, Height - 2), _borderRadius))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadowPath);
            }

            // Draw button background with gradient
            using (var brush = new LinearGradientBrush(ClientRectangle, 
                ControlPaint.Light(_currentColor, 0.1f), _currentColor, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // Draw text
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var textBrush = new SolidBrush(ForeColor);
            g.DrawString(Text, Font, textBrush, ClientRectangle, sf);
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Modern TextBox with rounded corners and custom styling
    /// </summary>
    public class ModernTextBox : UserControl
    {
        private TextBox _innerTextBox;
        private Color _borderColor = Color.FromArgb(52, 152, 219);
        private Color _focusBorderColor = Color.FromArgb(41, 128, 185);
        private Color _backgroundColor = Color.FromArgb(45, 52, 64);
        private int _borderRadius = 10;
        private int _borderSize = 2;
        private bool _isFocused = false;

        #pragma warning disable CS8765
        public override string Text
        {
            get => _innerTextBox.Text;
            set => _innerTextBox.Text = value;
        }
        #pragma warning restore CS8765

        public bool Multiline
        {
            get => _innerTextBox.Multiline;
            set
            {
                _innerTextBox.Multiline = value;
                _innerTextBox.ScrollBars = value ? ScrollBars.Vertical : ScrollBars.None;
            }
        }

        public bool ReadOnly
        {
            get => _innerTextBox.ReadOnly;
            set => _innerTextBox.ReadOnly = value;
        }

        public HorizontalAlignment TextAlign
        {
            get => _innerTextBox.TextAlign;
            set => _innerTextBox.TextAlign = value;
        }

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public new Color BackColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                _innerTextBox.BackColor = value;
                Invalidate();
            }
        }

        public new Color ForeColor
        {
            get => _innerTextBox.ForeColor;
            set => _innerTextBox.ForeColor = value;
        }

        public new Font Font
        {
            get => _innerTextBox.Font;
            set => _innerTextBox.Font = value;
        }

        public new event EventHandler? TextChanged
        {
            add => _innerTextBox.TextChanged += value;
            remove => _innerTextBox.TextChanged -= value;
        }

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            
            _innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = _backgroundColor,
                ForeColor = Color.White,
                Font = new Font("Consolas", 11F),
            };

            _innerTextBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _innerTextBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };

            Controls.Add(_innerTextBox);
            Size = new Size(200, 35);
            Padding = new Padding(_borderRadius / 2 + _borderSize);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int padding = _borderRadius / 2 + _borderSize + 4;
            _innerTextBox.Location = new Point(padding, Multiline ? padding : (Height - _innerTextBox.Height) / 2);
            _innerTextBox.Size = new Size(Width - padding * 2, Height - padding * 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.Transparent);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = GetRoundedRect(rect, _borderRadius);

            // Fill background
            using (var brush = new SolidBrush(_backgroundColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            Color borderColor = _isFocused ? _focusBorderColor : _borderColor;
            using (var pen = new Pen(borderColor, _borderSize))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public void Clear() => _innerTextBox.Clear();
        public void AppendText(string text) => _innerTextBox.AppendText(text);
    }

    /// <summary>
    /// Modern GroupBox with rounded corners
    /// </summary>
    public class ModernGroupBox : Panel
    {
        private string _title = "";
        private Color _borderColor = Color.FromArgb(52, 152, 219);
        private Color _titleColor = Color.White;
        private int _borderRadius = 15;
        private int _titleHeight = 28;

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public Color TitleColor
        {
            get => _titleColor;
            set { _titleColor = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public ModernGroupBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            Padding = new Padding(10, 35, 10, 10);
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? BackColor);

            var rect = new Rectangle(0, _titleHeight / 2, Width - 1, Height - _titleHeight / 2 - 1);
            using var path = GetRoundedRect(rect, _borderRadius);

            // Fill background
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (var pen = new Pen(_borderColor, 2))
            {
                g.DrawPath(pen, path);
            }

            // Draw title background
            if (!string.IsNullOrEmpty(_title))
            {
                var titleSize = g.MeasureString(_title, Font);
                var titleRect = new RectangleF(15, 0, titleSize.Width + 16, _titleHeight);
                
                using (var titleBg = new SolidBrush(Parent?.BackColor ?? BackColor))
                {
                    g.FillRectangle(titleBg, titleRect);
                }

                // Draw title icon
                using (var iconBrush = new SolidBrush(_borderColor))
                {
                    g.FillEllipse(iconBrush, 18, 6, 12, 12);
                }

                // Draw title text
                using (var textBrush = new SolidBrush(_titleColor))
                {
                    g.DrawString(_title, Font, textBrush, 35, (_titleHeight - titleSize.Height) / 2);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Gradient Panel with rounded corners
    /// </summary>
    public class GradientPanel : Panel
    {
        public Color StartColor { get; set; } = Color.FromArgb(44, 62, 80);
        public Color EndColor { get; set; } = Color.FromArgb(52, 73, 94);
        public float Angle { get; set; } = 45F;
        public int BorderRadius { get; set; } = 15;

        public GradientPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width, Height);
            using var path = GetRoundedRect(rect, BorderRadius);

            using (var brush = new LinearGradientBrush(rect, StartColor, EndColor, Angle))
            {
                g.FillPath(brush, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Modern Progress Bar with rounded corners and gradient
    /// </summary>
    public class ModernProgressBar : Control
    {
        private int _value = 0;
        private int _maximum = 100;
        private Color _progressColor = Color.FromArgb(46, 204, 113);
        private Color _backgroundColor = Color.FromArgb(45, 52, 64);
        private int _borderRadius = 8;
        private string _statusText = "";
        private bool _showPercentage = true;

        public int Value
        {
            get => _value;
            set { _value = Math.Max(0, Math.Min(value, _maximum)); Invalidate(); }
        }

        public int Maximum
        {
            get => _maximum;
            set { _maximum = Math.Max(1, value); Invalidate(); }
        }

        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; Invalidate(); }
        }

        public new Color BackColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; Invalidate(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; Invalidate(); }
        }

        public bool ShowPercentage
        {
            get => _showPercentage;
            set { _showPercentage = value; Invalidate(); }
        }

        public ModernProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            Size = new Size(300, 25);
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.Transparent);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var bgPath = GetRoundedRect(rect, _borderRadius);

            // Draw background
            using (var brush = new SolidBrush(_backgroundColor))
            {
                g.FillPath(brush, bgPath);
            }

            // Draw progress
            if (_value > 0)
            {
                float percent = (float)_value / _maximum;
                int progressWidth = Math.Max(_borderRadius * 2, (int)(Width * percent));
                var progressRect = new Rectangle(0, 0, progressWidth - 1, Height - 1);
                using var progressPath = GetRoundedRect(progressRect, _borderRadius);

                using (var brush = new LinearGradientBrush(progressRect,
                    ControlPaint.Light(_progressColor, 0.2f), _progressColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, progressPath);
                }

                // Shine effect
                var shineRect = new Rectangle(0, 0, progressWidth - 1, Height / 2);
                using (var shineBrush = new LinearGradientBrush(shineRect,
                    Color.FromArgb(50, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                {
                    g.FillRectangle(shineBrush, shineRect);
                }
            }

            // Draw text
            string text = _showPercentage ? $"{(_value * 100 / _maximum)}%" : _statusText;
            if (!string.IsNullOrEmpty(text))
            {
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(Color.White);
                g.DrawString(text, Font, textBrush, rect, sf);
            }

            // Draw border
            using (var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
            {
                g.DrawPath(pen, bgPath);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Modern TabControl with custom styling
    /// </summary>
    public class ModernTabControl : TabControl
    {
        private Color _tabBackColor = Color.FromArgb(30, 30, 46);
        private Color _selectedTabColor = Color.FromArgb(52, 152, 219);
        private Color _hoverTabColor = Color.FromArgb(45, 45, 65);
        private int _tabRadius = 12;

        public Color TabBackColor
        {
            get => _tabBackColor;
            set { _tabBackColor = value; Invalidate(); }
        }

        public Color SelectedTabColor
        {
            get => _selectedTabColor;
            set { _selectedTabColor = value; Invalidate(); }
        }

        public ModernTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            ItemSize = new Size(150, 45);
            SizeMode = TabSizeMode.Fixed;
            Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_tabBackColor);

            // Draw tabs
            for (int i = 0; i < TabCount; i++)
            {
                var tabRect = GetTabRect(i);
                bool isSelected = (SelectedIndex == i);
                
                // Tab background
                var tabPath = GetRoundedTabRect(tabRect, _tabRadius);
                Color tabColor = isSelected ? _selectedTabColor : _tabBackColor;
                
                using (var brush = new SolidBrush(tabColor))
                {
                    g.FillPath(brush, tabPath);
                }

                // Selected indicator
                if (isSelected)
                {
                    using (var pen = new Pen(_selectedTabColor, 3))
                    {
                        g.DrawLine(pen, tabRect.Left + 10, tabRect.Bottom - 2, tabRect.Right - 10, tabRect.Bottom - 2);
                    }
                }

                // Tab text
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(Color.White);
                g.DrawString(TabPages[i].Text, Font, textBrush, tabRect, sf);
            }

            // Draw content area border
            var contentRect = new Rectangle(0, ItemSize.Height, Width - 1, Height - ItemSize.Height - 1);
            using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
            {
                g.DrawRectangle(pen, contentRect);
            }
        }

        private GraphicsPath GetRoundedTabRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// File drop panel with visual feedback
    /// </summary>
    public class FileDropPanel : Panel
    {
        private bool _isDragging = false;
        private Color _borderColor = Color.FromArgb(52, 152, 219);
        private Color _dragColor = Color.FromArgb(46, 204, 113);
        private int _borderRadius = 15;
        private string _dropText = "Drag & Drop file here\nor click to browse";
        private Image? _fileTypeIcon = null;
        private string _fileName = "";
        private long _fileSize = 0;
        private string _filePath = "";

        public event EventHandler<string>? FileDropped;

        public string FileName => _fileName;
        public long FileSize => _fileSize;
        public string FilePath => _filePath;

        public string DropText
        {
            get => _dropText;
            set { _dropText = value; Invalidate(); }
        }

        public FileDropPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            AllowDrop = true;
            Cursor = Cursors.Hand;
            Size = new Size(300, 150);
            Font = new Font("Segoe UI", 10F);
        }

        public void SetFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                _filePath = filePath;
                _fileName = fi.Name;
                _fileSize = fi.Length;
                // Load icon based on file extension
                _fileTypeIcon = IconManager.GetFileTypeIcon(fi.Extension, 48);
                Invalidate();
                FileDropped?.Invoke(this, filePath);
            }
        }

        public void ClearFile()
        {
            _filePath = "";
            _fileName = "";
            _fileSize = 0;
            _fileTypeIcon = null;
            Invalidate();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
                _isDragging = true;
                Invalidate();
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            _isDragging = false;
            Invalidate();
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            _isDragging = false;
            
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                SetFile(files[0]);
            }
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            using var ofd = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Select a file to encrypt"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SetFile(ofd.FileName);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(Parent?.BackColor ?? BackColor);

            var rect = new Rectangle(2, 2, Width - 5, Height - 5);
            using var path = GetRoundedRect(rect, _borderRadius);

            // Fill background
            Color bgColor = _isDragging ? Color.FromArgb(40, _dragColor) : Color.FromArgb(35, 35, 50);
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
            }

            // Draw dashed border
            Color borderColor = _isDragging ? _dragColor : _borderColor;
            using (var pen = new Pen(borderColor, 2) { DashStyle = DashStyle.Dash })
            {
                g.DrawPath(pen, path);
            }

            // Draw content
            if (string.IsNullOrEmpty(_fileName))
            {
                // Draw folder icon from resources
                var folderIcon = IconManager.GetIcon(IconManager.IconType.Folder, 48);
                if (folderIcon != null)
                {
                    g.DrawImage(folderIcon, Width / 2 - 24, Height / 2 - 45, 48, 48);
                }
                else
                {
                    DrawDropIcon(g, Width / 2 - 20, Height / 2 - 35, 40, borderColor);
                }

                // Draw text
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
                var textRect = new Rectangle(0, Height / 2 + 10, Width, Height / 2 - 10);
                g.DrawString(_dropText, Font, textBrush, textRect, sf);
            }
            else
            {
                // Draw file type icon
                if (_fileTypeIcon != null)
                {
                    g.DrawImage(_fileTypeIcon, 20, Height / 2 - 24, 48, 48);
                }
                else
                {
                    DrawFileIcon(g, 20, Height / 2 - 25, 50, _borderColor);
                }

                using var textBrush = new SolidBrush(Color.White);
                using var sizeBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
                
                string displayName = _fileName.Length > 35 ? _fileName.Substring(0, 32) + "..." : _fileName;
                g.DrawString(displayName, new Font(Font, FontStyle.Bold), textBrush, 80, Height / 2 - 20);
                g.DrawString(FormatFileSize(_fileSize), Font, sizeBrush, 80, Height / 2 + 5);
            }
        }

        private void DrawDropIcon(Graphics g, int x, int y, int size, Color color)
        {
            using var pen = new Pen(color, 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            
            // Cloud shape
            int cloudY = y + size / 4;
            g.DrawArc(pen, x, cloudY, size / 2, size / 2, 180, 180);
            g.DrawArc(pen, x + size / 4, cloudY - size / 6, size / 2, size / 2, 180, 180);
            g.DrawArc(pen, x + size / 2, cloudY, size / 2, size / 2, 180, 180);
            
            // Arrow
            int arrowX = x + size / 2;
            int arrowY = y + size / 2;
            g.DrawLine(pen, arrowX, arrowY - 5, arrowX, arrowY + 15);
            g.DrawLine(pen, arrowX - 8, arrowY + 7, arrowX, arrowY + 15);
            g.DrawLine(pen, arrowX + 8, arrowY + 7, arrowX, arrowY + 15);
        }

        private void DrawFileIcon(Graphics g, int x, int y, int size, Color color)
        {
            using var pen = new Pen(color, 2) { LineJoin = LineJoin.Round };
            using var brush = new SolidBrush(Color.FromArgb(60, color));
            
            // File shape
            var points = new Point[]
            {
                new Point(x, y),
                new Point(x + size - 15, y),
                new Point(x + size, y + 15),
                new Point(x + size, y + size),
                new Point(x, y + size)
            };
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
            
            // Fold corner
            g.DrawLine(pen, x + size - 15, y, x + size - 15, y + 15);
            g.DrawLine(pen, x + size - 15, y + 15, x + size, y + 15);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Animated status label
    /// </summary>
    public class AnimatedLabel : Label
    {
        private System.Windows.Forms.Timer? _animTimer;
        private int _dotCount = 0;
        private string _baseText = "";
        private bool _isAnimating = false;
        #pragma warning disable CS0414
        private float _fadeOpacity = 1f;
        #pragma warning restore CS0414

        public bool IsAnimating => _isAnimating;

        public AnimatedLabel()
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            ForeColor = Color.FromArgb(46, 204, 113);
            AutoSize = true;
        }

        public void StartAnimation(string text)
        {
            _baseText = text;
            _isAnimating = true;
            _dotCount = 0;

            _animTimer?.Stop();
            _animTimer?.Dispose();
            
            _animTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _animTimer.Tick += (s, e) =>
            {
                _dotCount = (_dotCount + 1) % 4;
                Text = _baseText + new string('.', _dotCount);
            };
            _animTimer.Start();
        }

        public void StopAnimation(string? finalText = null)
        {
            _isAnimating = false;
            _animTimer?.Stop();
            _animTimer?.Dispose();
            Text = finalText ?? _baseText;
        }

        public void ShowSuccess(string message)
        {
            ForeColor = Color.FromArgb(46, 204, 113);
            Text = "✓ " + message;
            FadeOut(3000);
        }

        public void ShowError(string message)
        {
            ForeColor = Color.FromArgb(231, 76, 60);
            Text = "✗ " + message;
            FadeOut(5000);
        }

        private void FadeOut(int delay)
        {
            var timer = new System.Windows.Forms.Timer { Interval = delay };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                Text = "";
            };
            timer.Start();
        }
    }

    /// <summary>
    /// Hex viewer control for binary file preview
    /// </summary>
    public class HexViewer : Control
    {
        private byte[] _data = Array.Empty<byte>();
        private int _bytesPerLine = 16;
        private int _scrollOffset = 0;
        private VScrollBar _scrollBar;
        private Font _hexFont;
        private int _lineHeight = 20;

        public byte[] Data
        {
            get => _data;
            set
            {
                _data = value ?? Array.Empty<byte>();
                UpdateScrollBar();
                Invalidate();
            }
        }

        public HexViewer()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            _hexFont = new Font("Consolas", 10F);
            BackColor = Color.FromArgb(30, 30, 46);
            
            _scrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            _scrollBar.Scroll += (s, e) => { _scrollOffset = e.NewValue; Invalidate(); };
            Controls.Add(_scrollBar);
        }

        private void UpdateScrollBar()
        {
            int totalLines = (_data.Length + _bytesPerLine - 1) / _bytesPerLine;
            int visibleLines = (Height - 10) / _lineHeight;
            
            if (totalLines > visibleLines)
            {
                _scrollBar.Visible = true;
                _scrollBar.Maximum = totalLines - visibleLines + 10;
                _scrollBar.LargeChange = visibleLines;
            }
            else
            {
                _scrollBar.Visible = false;
                _scrollOffset = 0;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_scrollBar.Visible)
            {
                _scrollOffset = Math.Max(0, Math.Min(_scrollBar.Maximum - _scrollBar.LargeChange, 
                    _scrollOffset - (e.Delta / 40)));
                _scrollBar.Value = _scrollOffset;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);

            if (_data.Length == 0)
            {
                using var brush = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("No data to display", _hexFont, brush, ClientRectangle, sf);
                return;
            }

            int y = 5;
            int startLine = _scrollOffset;
            int visibleLines = (Height - 10) / _lineHeight;

            using var addressBrush = new SolidBrush(Color.FromArgb(155, 89, 182));
            using var hexBrush = new SolidBrush(Color.FromArgb(52, 152, 219));
            using var asciiBrush = new SolidBrush(Color.FromArgb(46, 204, 113));
            using var separatorPen = new Pen(Color.FromArgb(60, 255, 255, 255));

            for (int line = startLine; line < startLine + visibleLines && line * _bytesPerLine < _data.Length; line++)
            {
                int offset = line * _bytesPerLine;
                
                // Address
                g.DrawString($"{offset:X8}", _hexFont, addressBrush, 5, y);

                // Separator
                g.DrawLine(separatorPen, 80, y, 80, y + _lineHeight);

                // Hex values
                var hexBuilder = new System.Text.StringBuilder();
                var asciiBuilder = new System.Text.StringBuilder();

                for (int i = 0; i < _bytesPerLine && offset + i < _data.Length; i++)
                {
                    byte b = _data[offset + i];
                    hexBuilder.Append($"{b:X2} ");
                    asciiBuilder.Append(b >= 32 && b < 127 ? (char)b : '.');

                    if (i == 7) hexBuilder.Append(" ");
                }

                g.DrawString(hexBuilder.ToString(), _hexFont, hexBrush, 90, y);
                g.DrawLine(separatorPen, Width - 155, y, Width - 155, y + _lineHeight);
                g.DrawString(asciiBuilder.ToString(), _hexFont, asciiBrush, Width - 150, y);

                y += _lineHeight;
            }
        }
    }
}
