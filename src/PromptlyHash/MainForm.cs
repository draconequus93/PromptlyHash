namespace PromptlyHash;

public sealed class MainForm : Form
{
    private static readonly string DisplayVersion =
        typeof(MainForm).Assembly.GetName().Version?.ToString(3) ?? "unknown";

    private readonly TextBox _filePath = new();
    private readonly Button _browseButton = new();
    private readonly ComboBox _algorithm = new();
    private readonly Label _algorithmHint = new();
    private readonly RadioButton _generateMode = new();
    private readonly RadioButton _checkMode = new();
    private readonly Label _expectedLabel = new();
    private readonly TextBox _expectedHash = new();
    private readonly Button _actionButton = new();
    private readonly ProgressBar _progress = new();
    private readonly Panel _resultPanel = new();
    private readonly Label _resultTitle = new();
    private readonly TextBox _resultHash = new();
    private readonly Button _copyButton = new();
    private readonly Label _hint = new();
    private readonly Label _status = new();
    private readonly Panel _scrollHost = new();
    private readonly Panel _card = new();
    private readonly TableLayoutPanel _root = new();
    private readonly TableLayoutPanel _content = new();
    private CancellationTokenSource? _hashCancellation;
    private bool _closing;

    private static readonly Color Background = Color.FromArgb(246, 248, 252);
    private static readonly Color Primary = Color.FromArgb(40, 81, 163);
    private static readonly Color TextColor = Color.FromArgb(25, 35, 52);
    private static readonly Color Muted = Color.FromArgb(92, 105, 126);
    private static readonly Color Success = Color.FromArgb(24, 128, 80);
    private static readonly Color Danger = Color.FromArgb(190, 47, 55);
    private static readonly Color SelectorBackground = Color.FromArgb(228, 239, 250);

    public MainForm()
    {
        Text = $"PromptlyHash - v{DisplayVersion}";
        Icon = BrandLogo.CreateIcon();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(540, 480);
        Size = FitToScreen(new Size(860, 760));
        BackColor = Background;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BuildUi();
        UpdateMode();
        Resize += (_, _) => ApplyResponsiveSpacing();
        Shown += (_, _) => ApplyResponsiveSpacing();
    }

    private void BuildUi()
    {
        _root.Dock = DockStyle.Fill;
        _root.ColumnCount = 1;
        _root.RowCount = 3;
        _root.Padding = new Padding(36, 24, 36, 22);
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        Controls.Add(_root);

        BuildHeader();

        _scrollHost.Dock = DockStyle.Fill;
        _scrollHost.AutoScroll = true;
        _scrollHost.BackColor = Background;
        _scrollHost.Margin = new Padding(0, 8, 0, 14);
        _scrollHost.SizeChanged += (_, _) => ResizeCardToViewport();
        _root.Controls.Add(_scrollHost);

        _card.AutoSize = false;
        _card.BackColor = Color.White;
        _card.Padding = new Padding(28);
        _card.Location = Point.Empty;
        _scrollHost.Controls.Add(_card);

        _content.AutoSize = true;
        _content.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _content.Dock = DockStyle.Top;
        _content.ColumnCount = 1;
        _content.RowCount = 10;
        _content.Margin = Padding.Empty;
        _content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var index = 0; index < _content.RowCount; index++)
            _content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _card.Controls.Add(_content);

        _content.Controls.Add(MakeSectionLabel("FILE"));
        BuildFileRow(_content);

        _content.Controls.Add(MakeSectionLabel("ACTION"));
        BuildModeRow(_content);

        _content.Controls.Add(MakeSectionLabel("HASH ALGORITHM"));
        BuildAlgorithmSelector(_content);

        _expectedLabel.Text = "HASH TO CHECK";
        _expectedLabel.Font = new Font("Segoe UI Semibold", 8.5F);
        _expectedLabel.ForeColor = Muted;
        _expectedLabel.AutoSize = true;
        _expectedLabel.Margin = Padding.Empty;
        _content.Controls.Add(_expectedLabel);

        ConfigureTextBox(_expectedHash, "Paste the expected hash here");
        _expectedHash.MaxLength = 512;
        _expectedHash.AccessibleName = "Expected hash";
        _expectedHash.Margin = new Padding(0, 6, 0, 16);
        _expectedHash.TextChanged += (_, _) => ClearResult();
        _content.Controls.Add(_expectedHash);

        BuildActionArea(_content);
        BuildResultPanel();
        _content.Controls.Add(_resultPanel);

        BuildFooter();
        AcceptButton = _actionButton;
        ResizeCardToViewport();
    }

    private void BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var logo = new LogoControl { Margin = new Padding(0, 0, 12, 0), Anchor = AnchorStyles.Left | AnchorStyles.Top };
        header.Controls.Add(logo, 0, 0);

        var titles = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        titles.Controls.Add(new Label
        {
            Text = "PromptlyHash",
            Font = new Font("Segoe UI Semibold", 21F),
            ForeColor = BrandLogo.Navy,
            AutoSize = true,
            Margin = Padding.Empty
        });
        titles.Controls.Add(new Label
        {
            Text = "Generate or verify file checksums with confidence.",
            ForeColor = Muted,
            AutoSize = true,
            AutoEllipsis = true,
            Margin = new Padding(1, 1, 0, 0)
        });
        header.Controls.Add(titles, 1, 0);

        var aboutButton = new Button
        {
            Text = "About",
            AutoSize = true,
            MinimumSize = new Size(84, 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Margin = new Padding(12, 8, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Background,
            ForeColor = BrandLogo.Navy,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 9F)
        };
        aboutButton.FlatAppearance.BorderColor = Color.FromArgb(190, 199, 214);
        aboutButton.Click += (_, _) =>
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        };
        header.Controls.Add(aboutButton, 2, 0);
        _root.Controls.Add(header);
    }

    private void BuildFileRow(TableLayoutPanel content)
    {
        var fileRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Height = 44,
            Margin = new Padding(0, 6, 0, 18)
        };
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        ConfigureTextBox(_filePath, "Select a file to get started");
        _filePath.ReadOnly = true;
        _filePath.TabStop = false;
        _filePath.Margin = new Padding(0, 0, 10, 0);
        ConfigureButton(_browseButton, "Browse…", false);
        _browseButton.Click += BrowseFile;
        fileRow.Controls.Add(_filePath, 0, 0);
        fileRow.Controls.Add(_browseButton, 1, 0);
        content.Controls.Add(fileRow);
    }

    private void BuildModeRow(TableLayoutPanel content)
    {
        var modeRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 7, 0, 18)
        };
        _generateMode.Text = "Generate hash";
        _generateMode.AutoSize = true;
        _generateMode.Checked = true;
        _generateMode.Margin = new Padding(0, 4, 30, 4);
        _checkMode.Text = "Check hash";
        _checkMode.AutoSize = true;
        _checkMode.Margin = new Padding(0, 4, 0, 4);
        _generateMode.CheckedChanged += (_, _) => UpdateMode();
        modeRow.Controls.AddRange([_generateMode, _checkMode]);
        content.Controls.Add(modeRow);
    }

    private void BuildAlgorithmSelector(TableLayoutPanel content)
    {
        var selectorRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 6, 0, 16)
        };
        _algorithm.DropDownStyle = ComboBoxStyle.DropDownList;
        _algorithm.FlatStyle = FlatStyle.Standard;
        _algorithm.Items.AddRange(HashService.Algorithms.Cast<object>().ToArray());
        _algorithm.SelectedItem = "SHA-256";
        _algorithm.Width = 220;
        _algorithm.Height = 38;
        _algorithm.DropDownWidth = 220;
        _algorithm.BackColor = SelectorBackground;
        _algorithm.ForeColor = BrandLogo.Navy;
        _algorithm.Font = new Font("Segoe UI Semibold", 10F);
        _algorithm.Margin = Padding.Empty;
        _algorithm.AccessibleName = "Hash algorithm";
        _algorithm.SelectedIndexChanged += (_, _) =>
        {
            UpdateAlgorithmHint();
            ClearResult();
        };
        selectorRow.Controls.Add(_algorithm);
        _algorithmHint.AutoSize = true;
        _algorithmHint.ForeColor = Muted;
        _algorithmHint.Margin = new Padding(12, 7, 0, 0);
        selectorRow.Controls.Add(_algorithmHint);
        UpdateAlgorithmHint();
        content.Controls.Add(selectorRow);
    }

    private void BuildActionArea(TableLayoutPanel content)
    {
        var actionArea = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 12)
        };
        ConfigureButton(_actionButton, "Generate hash", true);
        _actionButton.Height = 44;
        _actionButton.Click += RunHashOperation;
        _progress.Dock = DockStyle.Top;
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 25;
        _progress.Height = 4;
        _progress.Visible = false;
        _progress.Margin = new Padding(0, 8, 0, 0);
        actionArea.Controls.Add(_actionButton);
        actionArea.Controls.Add(_progress);
        content.Controls.Add(actionArea);
    }

    private void BuildResultPanel()
    {
        _resultPanel.Dock = DockStyle.Top;
        _resultPanel.AutoSize = true;
        _resultPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _resultPanel.MinimumSize = new Size(0, 98);
        _resultPanel.BackColor = Color.FromArgb(238, 246, 255);
        _resultPanel.Padding = new Padding(16, 12, 16, 12);
        _resultPanel.Margin = new Padding(0, 2, 0, 0);
        _resultPanel.Visible = false;

        var resultLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty
        };
        resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _resultTitle.AutoSize = true;
        _resultTitle.Font = new Font("Segoe UI Semibold", 10F);
        _resultTitle.Margin = new Padding(0, 0, 0, 12);
        resultLayout.Controls.Add(_resultTitle, 0, 0);
        resultLayout.SetColumnSpan(_resultTitle, 2);

        _resultHash.ReadOnly = true;
        _resultHash.Multiline = true;
        _resultHash.WordWrap = true;
        _resultHash.BorderStyle = BorderStyle.None;
        _resultHash.BackColor = _resultPanel.BackColor;
        _resultHash.Font = new Font("Cascadia Mono", 9F);
        _resultHash.Dock = DockStyle.Fill;
        _resultHash.Margin = new Padding(0, 8, 12, 0);
        _resultHash.TabStop = false;
        resultLayout.Controls.Add(_resultHash, 0, 1);

        ConfigureButton(_copyButton, "Copy", false);
        _copyButton.Dock = DockStyle.Fill;
        _copyButton.MinimumSize = new Size(84, 34);
        _copyButton.Margin = Padding.Empty;
        _copyButton.Click += CopyHash;
        resultLayout.Controls.Add(_copyButton, 1, 1);
        _resultPanel.Controls.Add(resultLayout);
    }

    private void BuildFooter()
    {
        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 44,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _status.Text = "Ready";
        _status.AutoSize = true;
        _status.ForeColor = Muted;
        _status.Margin = new Padding(3, 0, 0, 5);
        footer.Controls.Add(_status);

        _hint.Text = "Hashes are calculated locally. Your files never leave this computer.";
        _hint.AutoSize = true;
        _hint.ForeColor = Muted;
        _hint.Font = new Font("Segoe UI", 8.5F);
        _hint.Margin = new Padding(3, 0, 0, 0);
        footer.Controls.Add(_hint);
        _root.Controls.Add(footer);
    }

    private async void RunHashOperation(object? sender, EventArgs e)
    {
        if (!File.Exists(_filePath.Text))
        {
            MessageBox.Show(this, "Please select a valid file first.", "File required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedAlgorithm = _algorithm.SelectedItem?.ToString() ?? "SHA-256";
        if (_checkMode.Checked && !HashService.IsValidHash(_expectedHash.Text, selectedAlgorithm))
        {
            var length = HashService.ExpectedHexLength(selectedAlgorithm);
            MessageBox.Show(
                this,
                $"Enter a valid {selectedAlgorithm} hash ({length} hexadecimal characters). Spaces, dashes, and colons are accepted.",
                "Invalid hash",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            _expectedHash.Focus();
            return;
        }

        SetWorking(true, _checkMode.Checked ? "Checking file hash…" : "Generating file hash…");
        ClearResult();
        var cancellation = new CancellationTokenSource();
        _hashCancellation = cancellation;
        try
        {
            var actualHash = await HashService.ComputeFileHashAsync(
                _filePath.Text,
                selectedAlgorithm,
                cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();

            if (_checkMode.Checked)
            {
                var matches = HashService.HashesMatch(_expectedHash.Text, actualHash);
                ShowResult(
                    matches ? "✓ Hash matches — the file is verified." : "✕ Hash does not match.",
                    actualHash,
                    matches ? Success : Danger);
                _status.Text = matches ? "Verification complete: match" : "Verification complete: mismatch";
            }
            else
            {
                ShowResult($"{selectedAlgorithm} hash generated", actualHash, Primary);
                _status.Text = "Hash generated successfully";
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            if (!_closing)
                _status.Text = "Operation canceled";
        }
        catch (UnauthorizedAccessException)
        {
            if (!_closing)
                ShowError("Windows denied access to this file. Choose a file you have permission to read.");
        }
        catch (IOException ex)
        {
            if (!_closing)
                ShowError($"The file could not be read.\n\n{ex.Message}");
        }
        catch (Exception ex)
        {
            if (!_closing)
                ShowError($"Hashing failed.\n\n{ex.Message}");
        }
        finally
        {
            if (ReferenceEquals(_hashCancellation, cancellation))
                _hashCancellation = null;
            cancellation.Dispose();

            if (!_closing && !IsDisposed)
                SetWorking(false);
        }
    }

    private void BrowseFile(object? sender, EventArgs e)
    {
        using var fileDialog = new OpenFileDialog
        {
            Title = "Select a file",
            Filter = "All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        if (fileDialog.ShowDialog(this) != DialogResult.OK)
            return;

        _filePath.Text = fileDialog.FileName;
        _status.Text = $"Selected: {Path.GetFileName(fileDialog.FileName)}";
        ClearResult();
    }

    private async void CopyHash(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_resultHash.Text))
            return;

        try
        {
            Clipboard.SetText(_resultHash.Text);
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            MessageBox.Show(
                this,
                "The clipboard is currently unavailable. Please try again.",
                "PromptlyHash",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _copyButton.Text = "Copied!";
        await Task.Delay(1400);
        if (!IsDisposed)
            _copyButton.Text = "Copy";
    }

    private void UpdateMode()
    {
        var checking = _checkMode.Checked;
        _expectedLabel.Visible = checking;
        _expectedHash.Visible = checking;
        _actionButton.Text = checking ? "Check hash" : "Generate hash";
        ClearResult();
        ResizeCardToViewport();
    }

    private void SetWorking(bool working, string? status = null)
    {
        UseWaitCursor = working;
        _progress.Visible = working;
        _browseButton.Enabled = !working;
        _actionButton.Enabled = !working;
        _algorithm.Enabled = !working;
        _generateMode.Enabled = !working;
        _checkMode.Enabled = !working;
        _expectedHash.Enabled = !working;
        if (status is not null)
            _status.Text = status;
    }

    private void ShowResult(string title, string hash, Color color)
    {
        _resultTitle.Text = title;
        _resultTitle.ForeColor = color;
        _resultHash.Text = hash;
        UpdateResultHashHeight();
        _resultPanel.Visible = true;
        _copyButton.Visible = true;
        _resultPanel.PerformLayout();
        ResizeCardToViewport();
        BeginInvoke(() => _scrollHost.ScrollControlIntoView(_resultPanel));
    }

    private void ClearResult()
    {
        _resultPanel.Visible = false;
        _resultHash.Text = string.Empty;
        ResizeCardToViewport();
    }

    private void ApplyResponsiveSpacing()
    {
        var compact = ClientSize.Width < 700;
        _root.Padding = compact
            ? new Padding(16, 16, 16, 14)
            : new Padding(36, 24, 36, 22);
        _card.Padding = compact ? new Padding(18) : new Padding(28);
        ResizeCardToViewport();
    }

    private void ResizeCardToViewport()
    {
        if (_scrollHost.ClientSize.Width <= 0)
            return;

        var availableWidth = _scrollHost.ClientSize.Width
            - (_scrollHost.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
        _card.Width = Math.Max(410, availableWidth);
        _content.Width = Math.Max(1, _card.ClientSize.Width);
        UpdateResultHashHeight();
        _content.PerformLayout();
        // WinForms cannot auto-size only one dimension, so width follows the viewport
        // while height follows the table's preferred content height.
        var contentHeight = _content.GetPreferredSize(new Size(_content.Width, 0)).Height;
        _card.Height = contentHeight + _card.Padding.Vertical;
    }

    private void UpdateAlgorithmHint()
    {
        var algorithm = _algorithm.SelectedItem?.ToString() ?? "SHA-256";
        _algorithmHint.Text = HashService.IsLegacyAlgorithm(algorithm)
            ? "Legacy algorithm — use only for compatibility"
            : "Recommended for file verification";
        _algorithmHint.ForeColor = HashService.IsLegacyAlgorithm(algorithm) ? Danger : Muted;
    }

    private void UpdateResultHashHeight()
    {
        if (string.IsNullOrEmpty(_resultHash.Text))
        {
            _resultHash.MinimumSize = new Size(0, 26);
            return;
        }

        var availableWidth = Math.Max(140, _card.ClientSize.Width
            - _card.Padding.Horizontal
            - _resultPanel.Padding.Horizontal
            - 106);
        var textWidth = TextRenderer.MeasureText(
            _resultHash.Text,
            _resultHash.Font,
            new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding).Width;
        var lineCount = Math.Clamp((int)Math.Ceiling((double)textWidth / availableWidth), 1, 4);
        _resultHash.MinimumSize = new Size(0, lineCount * _resultHash.Font.Height + 8);
    }

    private void ShowError(string message)
    {
        _status.Text = "Operation failed";
        MessageBox.Show(this, message, "PromptlyHash", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static Label MakeSectionLabel(string text) => new()
    {
        Text = text,
        Font = new Font("Segoe UI Semibold", 8.5F),
        ForeColor = Muted,
        AutoSize = true,
        Margin = Padding.Empty
    };

    private static void ConfigureTextBox(TextBox textBox, string placeholder)
    {
        textBox.Dock = DockStyle.Top;
        textBox.Height = 36;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.PlaceholderText = placeholder;
        textBox.BackColor = Color.White;
        textBox.ForeColor = TextColor;
    }

    private static void ConfigureButton(Button button, string text, bool primary)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 199, 214);
        button.BackColor = primary ? Primary : Color.White;
        button.ForeColor = primary ? Color.White : TextColor;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
    }

    private static Size FitToScreen(Size preferred)
    {
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        return new Size(
            Math.Min(preferred.Width, Math.Max(520, area.Width - 48)),
            Math.Min(preferred.Height, Math.Max(460, area.Height - 48)));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel)
            return;

        _closing = true;
        _hashCancellation?.Cancel();
    }
}
