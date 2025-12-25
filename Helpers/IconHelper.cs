using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cryptography_App.Helpers
{
    /// <summary>
    /// Programmatic icons (no external files).
    /// Updated: Better contrast on dark UI + white button icons + subtle shadow.
    /// </summary>
    public static class IconHelper
    {
        // ===== Palette tuned for WHITE dropdown menus =====
        private static readonly Color MenuYellow = Color.FromArgb(241, 196, 15);
        private static readonly Color MenuBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color MenuGreen = Color.FromArgb(46, 204, 113);
        private static readonly Color MenuOrange = Color.FromArgb(230, 126, 34);
        private static readonly Color MenuRed = Color.FromArgb(231, 76, 60);
        private static readonly Color MenuPurple = Color.FromArgb(155, 89, 182);

        // ===== Palette tuned for COLORED buttons / dark UI =====
        private static readonly Color BtnFg = Color.FromArgb(248, 249, 251); // near-white
        private static readonly Color BtnShadow = Color.FromArgb(110, 0, 0, 0);   // subtle shadow

        // =========================
        // Public API
        // =========================
        public static ImageList CreateMenuImageList()
        {
            var imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            imageList.Images.Add("file", CreateFolderIcon(16, MenuYellow, shadow: true));
            imageList.Images.Add("open", CreateOpenIcon(16, MenuYellow, shadow: true));
            imageList.Images.Add("save", CreateSaveIcon(16, MenuBlue, shadow: true));
            imageList.Images.Add("export", CreateArrowIcon(16, MenuGreen, up: true, shadow: true));
            imageList.Images.Add("import", CreateArrowIcon(16, MenuBlue, up: false, shadow: true));
            imageList.Images.Add("exit", CreateExitIcon(16, MenuRed, shadow: true));

            imageList.Images.Add("key", CreateKeyIcon(16, MenuYellow, shadow: true));
            imageList.Images.Add("clear", CreateTrashIcon(16, MenuRed, shadow: true));
            imageList.Images.Add("copy", CreateCopyIcon(16, MenuBlue, shadow: true));
            imageList.Images.Add("paste", CreatePasteIcon(16, MenuOrange, shadow: true));

            imageList.Images.Add("help", CreateCircleTextIcon(16, MenuPurple, "?", shadow: true));
            imageList.Images.Add("info", CreateCircleTextIcon(16, MenuBlue, "i", shadow: true));

            imageList.Images.Add("encrypt", CreateLockIcon(16, MenuGreen, locked: true, shadow: true));
            imageList.Images.Add("decrypt", CreateLockIcon(16, MenuOrange, locked: false, shadow: true));

            imageList.Images.Add("swap", CreateSwapIcon(16, MenuBlue, shadow: true));
            imageList.Images.Add("matrix", CreateMatrixIcon(16, MenuPurple, shadow: true));
            imageList.Images.Add("generate", CreateRefreshIcon(16, MenuBlue, shadow: true));

            return imageList;
        }

        public static ImageList CreateButtonImageList()
        {
            var imageList = new ImageList();
            imageList.ImageSize = new Size(24, 24);
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            // IMPORTANT: Button icons are WHITE so they pop on green/orange/blue/red buttons.
            imageList.Images.Add("encrypt", CreateLockIcon(24, BtnFg, locked: true, shadow: true, shadowColor: BtnShadow));
            imageList.Images.Add("decrypt", CreateLockIcon(24, BtnFg, locked: false, shadow: true, shadowColor: BtnShadow));
            imageList.Images.Add("swap", CreateSwapIcon(24, BtnFg, shadow: true, shadowColor: BtnShadow));
            imageList.Images.Add("clear", CreateTrashIcon(24, BtnFg, shadow: true, shadowColor: BtnShadow));
            imageList.Images.Add("matrix", CreateMatrixIcon(24, BtnFg, shadow: true, shadowColor: BtnShadow));
            imageList.Images.Add("generate", CreateRefreshIcon(24, BtnFg, shadow: true, shadowColor: BtnShadow));

            return imageList;
        }

        // =========================
        // Drawing helpers
        // =========================
        private static void SetupG(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }

        private static float Stroke(int size) => size <= 16 ? 1.8f : 2.6f;

        private static Pen MakePen(Color c, float w)
        {
            var p = new Pen(c, w);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }

        private static Color Alpha(Color c, int a) => Color.FromArgb(a, c);

        private static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = radius * 2f;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawWithShadow(Graphics g, Action draw, bool shadow, Color shadowColor)
        {
            if (shadow)
            {
                var state = g.Save();
                g.TranslateTransform(0.9f, 0.9f);
                using (var sp = MakePen(shadowColor, 1.2f))
                {
                    // Some icons use their own pens; we just offset-draw by calling draw() after translation
                }
                draw();
                g.Restore(state);
            }
            draw();
        }

        // =========================
        // Icons (modern + consistent)
        // =========================
        private static Bitmap CreateFolderIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float pad = size * 0.08f;
                var baseRect = new RectangleF(pad, size * 0.36f, size - 2 * pad, size * 0.52f);
                var tabRect = new RectangleF(pad, size * 0.22f, size * 0.38f, size * 0.22f);

                Action draw = () =>
                {
                    if (shadow)
                    {
                        using (var sb = new SolidBrush(Alpha(sh, 110)))
                        {
                            g.FillRectangle(sb, baseRect.X, baseRect.Y, baseRect.Width, baseRect.Height);
                            g.FillRectangle(sb, tabRect.X, tabRect.Y, tabRect.Width, tabRect.Height);
                        }
                    }

                    using (var br = new LinearGradientBrush(
                        new RectangleF(0, 0, size, size),
                        Alpha(color, 255),
                        Alpha(ControlPaint.Dark(color, 0.15f), 255),
                        LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(br, baseRect);
                        g.FillRectangle(br, tabRect);
                    }

                    using (var p = MakePen(ControlPaint.Dark(color, 0.25f), 1.2f))
                    {
                        g.DrawRectangle(p, baseRect.X, baseRect.Y, baseRect.Width, baseRect.Height);
                    }
                };

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateOpenIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            // Folder + down arrow into it (looks more "open/import")
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);
                float pad = size * 0.10f;

                // folder base
                var folder = CreateFolderIcon(size, color, shadow, shadowColor);
                g.DrawImageUnscaled(folder, 0, 0);

                // arrow
                Action drawArrow = () =>
                {
                    using (var p = MakePen(Color.White, w))
                    {
                        // white arrow overlay for clarity on yellow folder
                        float cx = size * 0.60f;
                        float top = size * 0.28f;
                        float bot = size * 0.72f;

                        g.DrawLine(p, cx, top, cx, bot);
                        g.DrawLine(p, cx, bot, cx - size * 0.12f, bot - size * 0.12f);
                        g.DrawLine(p, cx, bot, cx + size * 0.12f, bot - size * 0.12f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        float cx = size * 0.60f;
                        float top = size * 0.28f;
                        float bot = size * 0.72f;
                        g.DrawLine(p, cx, top, cx, bot);
                        g.DrawLine(p, cx, bot, cx - size * 0.12f, bot - size * 0.12f);
                        g.DrawLine(p, cx, bot, cx + size * 0.12f, bot - size * 0.12f);
                    }
                    g.Restore(state);
                }

                drawArrow();
            }
            return bmp;
        }

        private static Bitmap CreateSaveIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float pad = size * 0.14f;
                var r = new RectangleF(pad, pad, size - 2 * pad, size - 2 * pad);

                Action draw = () =>
                {
                    if (shadow)
                    {
                        using (var sb = new SolidBrush(Alpha(sh, 120)))
                        {
                            g.FillRectangle(sb, r.X, r.Y, r.Width, r.Height);
                        }
                    }

                    using (var br = new LinearGradientBrush(
                        new RectangleF(0, 0, size, size),
                        Alpha(color, 255),
                        Alpha(ControlPaint.Dark(color, 0.20f), 255),
                        LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(br, r);
                    }

                    // top shutter
                    using (var br2 = new SolidBrush(Alpha(Color.FromArgb(30, 30, 30), 220)))
                    {
                        g.FillRectangle(br2, r.X + r.Width * 0.18f, r.Y + r.Height * 0.10f, r.Width * 0.64f, r.Height * 0.26f);
                    }
                    // label
                    using (var br3 = new SolidBrush(Alpha(Color.White, 235)))
                    {
                        g.FillRectangle(br3, r.X + r.Width * 0.18f, r.Y + r.Height * 0.52f, r.Width * 0.64f, r.Height * 0.34f);
                    }
                    using (var p = MakePen(ControlPaint.Dark(color, 0.25f), 1.2f))
                    {
                        g.DrawRectangle(p, r.X, r.Y, r.Width, r.Height);
                    }
                };

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateArrowIcon(int size, Color color, bool up, bool shadow, Color? shadowColor = null)
        {
            // Export/Import arrow with baseline
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);
                float cx = size * 0.50f;
                float top = size * 0.18f;
                float mid = size * 0.52f;
                float bot = size * 0.82f;

                float arrowTipY = up ? top : bot;
                float arrowBaseY = up ? mid : mid;

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        // stem
                        g.DrawLine(p, cx, up ? bot : top, cx, up ? top : bot);
                        // head
                        if (up)
                        {
                            g.DrawLine(p, cx, top, cx - size * 0.18f, top + size * 0.18f);
                            g.DrawLine(p, cx, top, cx + size * 0.18f, top + size * 0.18f);
                        }
                        else
                        {
                            g.DrawLine(p, cx, bot, cx - size * 0.18f, bot - size * 0.18f);
                            g.DrawLine(p, cx, bot, cx + size * 0.18f, bot - size * 0.18f);
                        }

                        // baseline
                        g.DrawLine(p, size * 0.18f, bot, size * 0.82f, bot);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        g.DrawLine(p, cx, up ? bot : top, cx, up ? top : bot);
                        if (up)
                        {
                            g.DrawLine(p, cx, top, cx - size * 0.18f, top + size * 0.18f);
                            g.DrawLine(p, cx, top, cx + size * 0.18f, top + size * 0.18f);
                        }
                        else
                        {
                            g.DrawLine(p, cx, bot, cx - size * 0.18f, bot - size * 0.18f);
                            g.DrawLine(p, cx, bot, cx + size * 0.18f, bot - size * 0.18f);
                        }
                        g.DrawLine(p, size * 0.18f, bot, size * 0.82f, bot);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateExitIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        g.DrawLine(p, size * 0.22f, size * 0.22f, size * 0.78f, size * 0.78f);
                        g.DrawLine(p, size * 0.78f, size * 0.22f, size * 0.22f, size * 0.78f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        g.DrawLine(p, size * 0.22f, size * 0.22f, size * 0.78f, size * 0.78f);
                        g.DrawLine(p, size * 0.78f, size * 0.22f, size * 0.22f, size * 0.78f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateKeyIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        // head
                        float r = size * 0.22f;
                        g.DrawEllipse(p, size * 0.12f, size * 0.30f, r, r);
                        // shaft
                        g.DrawLine(p, size * 0.34f, size * 0.41f, size * 0.86f, size * 0.41f);
                        // teeth
                        g.DrawLine(p, size * 0.72f, size * 0.41f, size * 0.72f, size * 0.62f);
                        g.DrawLine(p, size * 0.58f, size * 0.41f, size * 0.58f, size * 0.56f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        float r = size * 0.22f;
                        g.DrawEllipse(p, size * 0.12f, size * 0.30f, r, r);
                        g.DrawLine(p, size * 0.34f, size * 0.41f, size * 0.86f, size * 0.41f);
                        g.DrawLine(p, size * 0.72f, size * 0.41f, size * 0.72f, size * 0.62f);
                        g.DrawLine(p, size * 0.58f, size * 0.41f, size * 0.58f, size * 0.56f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateTrashIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        float x = size * 0.28f;
                        float y = size * 0.28f;
                        float ww = size * 0.44f;
                        float hh = size * 0.52f;

                        // can body
                        g.DrawRectangle(p, x, y + size * 0.12f, ww, hh);
                        // lid line
                        g.DrawLine(p, x - size * 0.10f, y + size * 0.12f, x + ww + size * 0.10f, y + size * 0.12f);
                        // handle
                        g.DrawLine(p, x + ww * 0.30f, y, x + ww * 0.70f, y);
                        // inner lines
                        g.DrawLine(p, x + ww * 0.30f, y + size * 0.24f, x + ww * 0.30f, y + hh + size * 0.06f);
                        g.DrawLine(p, x + ww * 0.50f, y + size * 0.24f, x + ww * 0.50f, y + hh + size * 0.06f);
                        g.DrawLine(p, x + ww * 0.70f, y + size * 0.24f, x + ww * 0.70f, y + hh + size * 0.06f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        float x = size * 0.28f;
                        float y = size * 0.28f;
                        float ww = size * 0.44f;
                        float hh = size * 0.52f;

                        g.DrawRectangle(p, x, y + size * 0.12f, ww, hh);
                        g.DrawLine(p, x - size * 0.10f, y + size * 0.12f, x + ww + size * 0.10f, y + size * 0.12f);
                        g.DrawLine(p, x + ww * 0.30f, y, x + ww * 0.70f, y);
                        g.DrawLine(p, x + ww * 0.30f, y + size * 0.24f, x + ww * 0.30f, y + hh + size * 0.06f);
                        g.DrawLine(p, x + ww * 0.50f, y + size * 0.24f, x + ww * 0.50f, y + hh + size * 0.06f);
                        g.DrawLine(p, x + ww * 0.70f, y + size * 0.24f, x + ww * 0.70f, y + hh + size * 0.06f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateCopyIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = 1.6f;

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        g.DrawRectangle(p, size * 0.28f, size * 0.18f, size * 0.52f, size * 0.52f);
                        g.DrawRectangle(p, size * 0.18f, size * 0.28f, size * 0.52f, size * 0.52f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        g.DrawRectangle(p, size * 0.28f, size * 0.18f, size * 0.52f, size * 0.52f);
                        g.DrawRectangle(p, size * 0.18f, size * 0.28f, size * 0.52f, size * 0.52f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreatePasteIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = 1.8f;

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        g.DrawRectangle(p, size * 0.24f, size * 0.22f, size * 0.52f, size * 0.62f);
                        g.DrawRectangle(p, size * 0.36f, size * 0.12f, size * 0.28f, size * 0.18f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 140), w + 0.8f))
                    {
                        g.DrawRectangle(p, size * 0.24f, size * 0.22f, size * 0.52f, size * 0.62f);
                        g.DrawRectangle(p, size * 0.36f, size * 0.12f, size * 0.28f, size * 0.18f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateCircleTextIcon(int size, Color color, string text, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                var circle = new RectangleF(1.5f, 1.5f, size - 3f, size - 3f);

                if (shadow)
                {
                    using (var sb = new SolidBrush(Alpha(sh, 120)))
                    {
                        g.FillEllipse(sb, circle.X + 0.8f, circle.Y + 0.8f, circle.Width, circle.Height);
                    }
                }

                using (var br = new SolidBrush(color))
                    g.FillEllipse(br, circle);

                using (var font = new Font("Segoe UI", size * 0.62f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var br = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(text, font, br, new RectangleF(0, 0, size, size), sf);
                }
            }
            return bmp;
        }

        private static Bitmap CreateLockIcon(int size, Color color, bool locked, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);
                float pad = size * 0.18f;

                var body = new RectangleF(pad, size * 0.52f, size - 2 * pad, size * 0.34f);
                float radius = size * 0.10f;

                // Shackle box
                var arcRect = new RectangleF(body.X + body.Width * 0.18f, size * 0.14f, body.Width * 0.64f, size * 0.50f);

                Action draw = () =>
                {
                    // Shadow (fill)
                    if (shadow)
                    {
                        using (var sb = new SolidBrush(Alpha(sh, 130)))
                        using (var pathS = RoundedRect(new RectangleF(body.X + 0.8f, body.Y + 0.8f, body.Width, body.Height), radius))
                        {
                            g.FillPath(sb, pathS);
                        }
                    }

                    // Body (slightly filled for better visibility)
                    using (var fill = new SolidBrush(Alpha(color, size <= 16 ? 210 : 230)))
                    using (var path = RoundedRect(body, radius))
                    {
                        g.FillPath(fill, path);
                    }

                    // Outline
                    using (var p = MakePen(color, w))
                    using (var path = RoundedRect(body, radius))
                    {
                        g.DrawPath(p, path);
                    }

                    // Shackle
                    if (shadow)
                    {
                        var state = g.Save();
                        g.TranslateTransform(0.8f, 0.8f);
                        using (var sp = MakePen(Alpha(sh, 150), w + 0.8f))
                        {
                            g.DrawArc(sp, arcRect, 180, 180);
                            if (locked)
                            {
                                g.DrawLine(sp, arcRect.X, arcRect.Y + arcRect.Height * 0.50f, arcRect.X, body.Y);
                                g.DrawLine(sp, arcRect.Right, arcRect.Y + arcRect.Height * 0.50f, arcRect.Right, body.Y);
                            }
                            else
                            {
                                // open: only right side connected
                                g.DrawLine(sp, arcRect.Right, arcRect.Y + arcRect.Height * 0.50f, arcRect.Right, body.Y);
                            }
                        }
                        g.Restore(state);
                    }

                    using (var p = MakePen(color, w))
                    {
                        g.DrawArc(p, arcRect, 180, 180);

                        if (locked)
                        {
                            g.DrawLine(p, arcRect.X, arcRect.Y + arcRect.Height * 0.50f, arcRect.X, body.Y);
                            g.DrawLine(p, arcRect.Right, arcRect.Y + arcRect.Height * 0.50f, arcRect.Right, body.Y);
                        }
                        else
                        {
                            // open shackle: shift left side outward for "open" feel
                            g.DrawLine(p, arcRect.Right, arcRect.Y + arcRect.Height * 0.50f, arcRect.Right, body.Y);
                        }
                    }

                    // keyhole
                    using (var br = new SolidBrush(Alpha(Color.Black, size <= 16 ? 55 : 65)))
                    {
                        float kx = body.X + body.Width * 0.50f;
                        float ky = body.Y + body.Height * 0.55f;
                        float rr = size * 0.06f;
                        g.FillEllipse(br, kx - rr, ky - rr, rr * 2, rr * 2);
                        g.FillRectangle(br, kx - rr * 0.6f, ky, rr * 1.2f, rr * 2.2f);
                    }
                };

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateSwapIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        // top arrow ->
                        float y1 = size * 0.35f;
                        g.DrawLine(p, size * 0.18f, y1, size * 0.78f, y1);
                        g.DrawLine(p, size * 0.78f, y1, size * 0.65f, y1 - size * 0.10f);
                        g.DrawLine(p, size * 0.78f, y1, size * 0.65f, y1 + size * 0.10f);

                        // bottom arrow <-
                        float y2 = size * 0.65f;
                        g.DrawLine(p, size * 0.82f, y2, size * 0.22f, y2);
                        g.DrawLine(p, size * 0.22f, y2, size * 0.35f, y2 - size * 0.10f);
                        g.DrawLine(p, size * 0.22f, y2, size * 0.35f, y2 + size * 0.10f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 150), w + 0.8f))
                    {
                        float y1 = size * 0.35f;
                        g.DrawLine(p, size * 0.18f, y1, size * 0.78f, y1);
                        g.DrawLine(p, size * 0.78f, y1, size * 0.65f, y1 - size * 0.10f);
                        g.DrawLine(p, size * 0.78f, y1, size * 0.65f, y1 + size * 0.10f);

                        float y2 = size * 0.65f;
                        g.DrawLine(p, size * 0.82f, y2, size * 0.22f, y2);
                        g.DrawLine(p, size * 0.22f, y2, size * 0.35f, y2 - size * 0.10f);
                        g.DrawLine(p, size * 0.22f, y2, size * 0.35f, y2 + size * 0.10f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }

        private static Bitmap CreateMatrixIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                int grid = 3;
                float pad = size * 0.16f;
                float gap = size * 0.06f;
                float cell = (size - 2 * pad - (grid - 1) * gap) / grid;
                float radius = size * 0.08f;

                for (int r = 0; r < grid; r++)
                {
                    for (int c = 0; c < grid; c++)
                    {
                        var rect = new RectangleF(pad + c * (cell + gap), pad + r * (cell + gap), cell, cell);

                        if (shadow)
                        {
                            using (var sb = new SolidBrush(Alpha(sh, 120)))
                            using (var pathS = RoundedRect(new RectangleF(rect.X + 0.8f, rect.Y + 0.8f, rect.Width, rect.Height), radius))
                                g.FillPath(sb, pathS);
                        }

                        using (var br = new SolidBrush(Alpha(color, 230)))
                        using (var path = RoundedRect(rect, radius))
                            g.FillPath(br, path);
                    }
                }
            }
            return bmp;
        }

        private static Bitmap CreateRefreshIcon(int size, Color color, bool shadow, Color? shadowColor = null)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                SetupG(g);
                g.Clear(Color.Transparent);

                var sh = shadowColor ?? Color.FromArgb(90, 0, 0, 0);
                float w = Stroke(size);
                var rect = new RectangleF(size * 0.18f, size * 0.18f, size * 0.64f, size * 0.64f);

                Action draw = () =>
                {
                    using (var p = MakePen(color, w))
                    {
                        g.DrawArc(p, rect, 35, 280);

                        // arrow head
                        float ax = size * 0.82f;
                        float ay = size * 0.46f;
                        g.DrawLine(p, ax, ay, ax - size * 0.16f, ay - size * 0.06f);
                        g.DrawLine(p, ax, ay, ax - size * 0.06f, ay - size * 0.16f);
                    }
                };

                if (shadow)
                {
                    var state = g.Save();
                    g.TranslateTransform(0.8f, 0.8f);
                    using (var p = MakePen(Alpha(sh, 150), w + 0.8f))
                    {
                        g.DrawArc(p, rect, 35, 280);
                        float ax = size * 0.82f;
                        float ay = size * 0.46f;
                        g.DrawLine(p, ax, ay, ax - size * 0.16f, ay - size * 0.06f);
                        g.DrawLine(p, ax, ay, ax - size * 0.06f, ay - size * 0.16f);
                    }
                    g.Restore(state);
                }

                draw();
            }
            return bmp;
        }
    }
}
