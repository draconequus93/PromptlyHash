using System.Reflection;
using System.Drawing;
using PromptlyHash;

internal static class Program
{
    private static readonly List<string> Failures = [];
    private static int _checkCount;

    [STAThread]
    private static async Task<int> Main()
    {
        Check("Normalization", HashService.NormalizeHash("AA-bb: cc") == "AAbbcc");
        Check("Case-insensitive fixed-time comparison", HashService.HashesMatch("aa-bb", "AABB"));
        Check("Invalid hash comparison", !HashService.HashesMatch("not-hex", "not-hex"));
        Check("Different-length comparison", !HashService.HashesMatch("AA", "AAAA"));
        Check("Valid SHA-256", HashService.IsValidHash(new string('a', 64), "SHA-256"));
        Check("Invalid SHA-256 length", !HashService.IsValidHash(new string('a', 63), "SHA-256"));
        Check("Invalid hex", !HashService.IsValidHash(new string('z', 64), "SHA-256"));
        Check("MD5 is marked legacy", HashService.IsLegacyAlgorithm("MD5"));
        Check("SHA-1 is marked legacy", HashService.IsLegacyAlgorithm("SHA-1"));
        Check("SHA-256 is not marked legacy", !HashService.IsLegacyAlgorithm("SHA-256"));

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "abc");
            Check("MD5 file vector", await HashService.ComputeFileHashAsync(tempFile, "MD5") ==
                "900150983CD24FB0D6963F7D28E17F72");
            Check("Async file hash", await HashService.ComputeFileHashAsync(tempFile, "SHA-1") ==
                "A9993E364706816ABA3E25717850C26C9CD0D89D");
            Check("SHA-256 file vector", await HashService.ComputeFileHashAsync(tempFile, "SHA-256") ==
                "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD");

            using var canceled = new CancellationTokenSource();
            canceled.Cancel();
            await CheckThrowsAsync<OperationCanceledException>(
                "Canceled hashing",
                () => HashService.ComputeFileHashAsync(tempFile, "SHA-256", canceled.Token));
            await CheckThrowsAsync<ArgumentException>(
                "Unsupported algorithm",
                () => HashService.ComputeFileHashAsync(tempFile, "SHA-999"));
        }
        finally
        {
            File.Delete(tempFile);
        }

        CheckLayoutAtSize(new Size(820, 660), "standard");
        CheckLayoutAtSize(new Size(540, 480), "compact");
        CheckAboutContent();

        if (Failures.Count > 0)
        {
            Console.Error.WriteLine($"{Failures.Count} test(s) failed:");
            Failures.ForEach(Console.Error.WriteLine);
            return 1;
        }

        Console.WriteLine($"All {_checkCount} tests passed.");
        return 0;
    }

    private static void CheckLayoutAtSize(Size size, string label)
    {
        using var form = new MainForm
        {
            Size = size,
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        Application.DoEvents();

        if (label == "standard")
        {
            var version = typeof(MainForm).Assembly.GetName().Version?.ToString(3) ?? "unknown";
            Check("Window title includes version", form.Text == $"PromptlyHash - v{version}");
        }

        var checkMode = GetField<RadioButton>(form, "_checkMode");
        checkMode.Checked = true;
        var showResult = typeof(MainForm).GetMethod("ShowResult", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ShowResult was not found.");
        showResult.Invoke(form, ["✓ Hash matches — the file is verified.", new string('A', 128), Color.Green]);
        Application.DoEvents();

        var result = GetField<Panel>(form, "_resultPanel");
        var scrollHost = GetField<Panel>(form, "_scrollHost");
        var algorithm = GetField<ComboBox>(form, "_algorithm");
        var algorithmHint = GetField<Label>(form, "_algorithmHint");

        Check($"{label} result is visible", result.Visible);
        Check($"{label} result has full height", result.Height >= 98);
        Check($"{label} selector is visually distinct", algorithm.BackColor != form.BackColor);
        if (size.Height <= 480)
            Check($"{label} layout scrolls instead of clipping", scrollHost.VerticalScroll.Visible);

        var previewPath = Environment.GetEnvironmentVariable("PROMPTLYHASH_PREVIEW");
        if (label == "standard" && !string.IsNullOrWhiteSpace(previewPath))
        {
            using var preview = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(preview, new Rectangle(Point.Empty, form.Size));
            preview.Save(previewPath);
        }

        algorithm.SelectedItem = "MD5";
        Check($"{label} legacy warning is visible", algorithmHint.Text.Contains("Legacy", StringComparison.Ordinal));
        form.Close();
    }

    private static T GetField<T>(MainForm form, string name) where T : class =>
        (T)(typeof(MainForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(form)
            ?? throw new InvalidOperationException($"{name} was not found."));

    private static void CheckAboutContent()
    {
        var aboutType = typeof(HashService).Assembly.GetType("PromptlyHash.AboutForm")
            ?? throw new InvalidOperationException("AboutForm was not found.");
        using var about = (Form)(Activator.CreateInstance(aboutType)
            ?? throw new InvalidOperationException("AboutForm could not be created."));
        var document = Descendants(about).OfType<RichTextBox>().Single();
        Check("About version token is resolved", !document.Text.Contains("{{VERSION}}", StringComparison.Ordinal));
        Check("About developer credit is present", document.Text.Contains("Draconequus", StringComparison.Ordinal));
        Check("About AI disclosure is present", document.Text.Contains("OpenAI Codex using GPT-5.6 Sol", StringComparison.Ordinal));
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }

    private static void Check(string name, bool condition)
    {
        _checkCount++;
        if (!condition)
            Failures.Add($"- {name}");
    }

    private static async Task CheckThrowsAsync<TException>(string name, Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
            Check(name, false);
        }
        catch (TException)
        {
            Check(name, true);
        }
    }
}
