using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace PromptlyHash;

internal sealed partial class AboutForm : Form
{
    private static readonly Color TextColor = Color.FromArgb(25, 35, 52);

    public AboutForm()
    {
        Text = "About PromptlyHash";
        Icon = BrandLogo.CreateIcon();
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(480, 420);
        Size = FitToScreen(new Size(660, 650));
        BackColor = Color.FromArgb(246, 248, 252);
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(24)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var document = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            ForeColor = TextColor,
            DetectUrls = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Padding = new Padding(18),
            TabStop = true
        };
        document.LinkClicked += (_, e) => OpenLink(e.LinkText);
        root.Controls.Add(document);

        var close = new Button
        {
            Text = "Close",
            Anchor = AnchorStyles.Right,
            AutoSize = true,
            MinimumSize = new Size(100, 38),
            Margin = new Padding(0, 16, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = BrandLogo.Navy,
            ForeColor = Color.White
        };
        close.FlatAppearance.BorderSize = 0;
        close.Click += (_, _) => Close();
        root.Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;

        RenderMarkdown(document, LoadAboutMarkdown());
    }

    private static string LoadAboutMarkdown()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("PromptlyHash.About.md");
        if (stream is null)
            return "# About PromptlyHash\n\nAbout information is unavailable.";
        using var reader = new StreamReader(stream);
        var version = Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString(3) ?? "unknown";
        return reader.ReadToEnd().Replace("{{VERSION}}", version, StringComparison.Ordinal);
    }

    private static void RenderMarkdown(RichTextBox box, string markdown)
    {
        // The embedded About file intentionally supports a small, predictable Markdown subset.
        box.Clear();
        foreach (var rawLine in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (line == "---")
            {
                Append(box, "────────────────────────────\n", 9, FontStyle.Regular, Color.FromArgb(190, 199, 214));
            }
            else if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                Append(box, CleanInline(line[2..]) + "\n", 20, FontStyle.Bold, BrandLogo.Navy);
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Append(box, "\n" + CleanInline(line[3..]) + "\n", 12, FontStyle.Bold, BrandLogo.Navy);
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                Append(box, "  •  " + CleanInline(line[2..]) + "\n", 10, FontStyle.Regular, TextColor);
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                Append(box, "\n", 10, FontStyle.Regular, TextColor);
            }
            else
            {
                Append(box, CleanInline(line) + "\n", 10, FontStyle.Regular, TextColor);
            }
        }
        box.SelectionStart = 0;
        box.ScrollToCaret();
    }

    private static string CleanInline(string value)
    {
        var withoutEmphasis = value.Replace("**", "").Replace("__", "").Replace("`", "");
        return MarkdownLinkRegex().Replace(withoutEmphasis, "$1 ($2)");
    }

    private static void Append(RichTextBox box, string text, float size, FontStyle style, Color color)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        using var font = new Font("Segoe UI", size, style);
        box.SelectionFont = font;
        box.SelectionColor = color;
        box.AppendText(text);
    }

    private void OpenLink(string? linkText)
    {
        if (!Uri.TryCreate(linkText, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(
                this,
                "Windows could not open this link.",
                "PromptlyHash",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static Size FitToScreen(Size preferred)
    {
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        return new Size(
            Math.Min(preferred.Width, Math.Max(420, area.Width - 48)),
            Math.Min(preferred.Height, Math.Max(360, area.Height - 48)));
    }

    [GeneratedRegex(@"\[([^\]]+)\]\((https?://[^)]+)\)")]
    private static partial Regex MarkdownLinkRegex();
}
