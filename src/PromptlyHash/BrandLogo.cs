using System.Drawing.Drawing2D;

namespace PromptlyHash;

internal static class BrandLogo
{
    public static readonly Color Navy = Color.FromArgb(23, 54, 93);
    public static readonly Color Cyan = Color.FromArgb(39, 194, 208);

    public static Icon CreateIcon(int size = 64)
    {
        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Draw(graphics, new Rectangle(0, 0, size, size));
        var handle = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(handle);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    public static void Draw(Graphics graphics, Rectangle bounds)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var state = graphics.Save();
        var scale = Math.Min(bounds.Width, bounds.Height) / 256f;
        try
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(bounds.X, bounds.Y);
            graphics.ScaleTransform(scale, scale);

            using var background = RoundedRectangle(new RectangleF(0, 0, 256, 256), 58);
            using var navyBrush = new SolidBrush(Navy);
            using var cyanBrush = new SolidBrush(Cyan);
            using var hashPen = new Pen(Color.White, 22)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.FillPath(navyBrush, background);
            graphics.DrawLine(hashPen, 92, 63, 79, 193);
            graphics.DrawLine(hashPen, 158, 63, 145, 193);
            graphics.DrawLine(hashPen, 59, 108, 199, 108);
            graphics.DrawLine(hashPen, 54, 158, 194, 158);

            graphics.FillEllipse(cyanBrush, 150, 150, 82, 82);
            using var checkPen = new Pen(Navy, 12)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            graphics.DrawLines(checkPen, [new(170, 191), new(184, 205), new(213, 174)]);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private static GraphicsPath RoundedRectangle(RectangleF rectangle, float radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = false)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}

internal sealed class LogoControl : Control
{
    public LogoControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        Size = new Size(54, 54);
        AccessibleName = "PromptlyHash logo";
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        BrandLogo.Draw(e.Graphics, ClientRectangle);
    }
}
