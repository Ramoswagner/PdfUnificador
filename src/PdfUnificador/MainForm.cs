using System.Diagnostics;

namespace PdfUnificador;

/// <summary>
/// Formulário principal do PDF Unificador.
/// Dark theme com drag-and-drop, reordenação e progress assíncrono.
/// </summary>
public partial class MainForm : Form
{
    // ── State ─────────────────────────────────────────────────────
    private readonly List<string> _files = [];
    private CancellationTokenSource? _cts;

    // ── Controls (declarados aqui, inicializados em InitializeComponents) ──
    private Panel        _headerPanel   = null!;
    private Label        _titleLabel    = null!;
    private Label        _subtitleLabel = null!;
    private Panel        _dropZone      = null!;
    private Label        _dropLabel     = null!;
    private ListView     _fileList      = null!;
    private Panel        _btnPanel      = null!;
    private DarkButton   _btnAddFiles   = null!;
    private DarkButton   _btnRemove     = null!;
    private DarkButton   _btnUp         = null!;
    private DarkButton   _btnDown       = null!;
    private DarkButton   _btnClearAll   = null!;
    private Panel        _outputPanel   = null!;
    private Label        _lblOutput     = null!;
    private TextBox      _txtOutput     = null!;
    private DarkButton   _btnBrowse     = null!;
    private ProgressBar  _progressBar   = null!;
    private Label        _statusLabel   = null!;
    private DarkButton   _btnMerge      = null!;
    private DarkButton   _btnCancel     = null!;
    private Label        _countLabel    = null!;

    public MainForm()
    {
        InitializeComponents();
        Program.EnableDarkTitleBar(Handle);
        SetStatus("Pronto. Adicione arquivos PDF para começar.", isError: false);
    }

    // ════════════════════════════════════════════════════════════
    //  UI CONSTRUCTION
    // ════════════════════════════════════════════════════════════

    private void InitializeComponents()
    {
        SuspendLayout();

        // ── Form ───────────────────────────────────────────────
        Text            = "PDF Unificador";
        Size            = new Size(780, 640);
        MinimumSize     = new Size(680, 560);
        BackColor       = AppTheme.BgDeep;
        ForeColor       = AppTheme.TextPrimary;
        Font            = AppTheme.FontBody;
        StartPosition   = FormStartPosition.CenterScreen;
        DoubleBuffered  = true;

        // ── Header ─────────────────────────────────────────────
        _headerPanel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 72,
            BackColor = AppTheme.BgPanel,
            Padding   = new Padding(24, 0, 24, 0),
        };
        _headerPanel.Paint += (s, e) =>
        {
            // linha inferior sutil
            using var pen = new Pen(AppTheme.Border);
            e.Graphics.DrawLine(pen, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
        };

        _titleLabel = new Label
        {
            Text      = "PDF Unificador",
            Font      = AppTheme.FontTitle,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(24, 14),
        };

        _subtitleLabel = new Label
        {
            Text      = "Combine múltiplos PDFs em um único arquivo",
            Font      = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(26, 46),
        };

        _countLabel = new Label
        {
            Text      = "0 arquivos",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.Accent,
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
        };
        _countLabel.Location = new Point(_headerPanel.Width - 100, 28);
        _countLabel.Anchor   = AnchorStyles.Top | AnchorStyles.Right;

        _headerPanel.Controls.AddRange([_titleLabel, _subtitleLabel, _countLabel]);

        // ── Content wrapper ────────────────────────────────────
        var content = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(20, 16, 20, 16),
        };

        // ── Drop Zone ──────────────────────────────────────────
        _dropZone = new Panel
        {
            Height      = 64,
            Dock        = DockStyle.Top,
            BackColor   = AppTheme.BgCard,
            Cursor      = Cursors.Hand,
            AllowDrop   = true,
            Margin      = new Padding(0, 0, 0, 10),
        };
        _dropZone.Paint += DropZone_Paint;
        _dropZone.DragEnter += DropZone_DragEnter;
        _dropZone.DragDrop  += DropZone_DragDrop;
        _dropZone.Click     += (_, _) => OpenFileDialog();
        _dropZone.MouseEnter += (_, _) => { _dropZone.BackColor = AppTheme.BgHover; _dropZone.Invalidate(); };
        _dropZone.MouseLeave += (_, _) => { _dropZone.BackColor = AppTheme.BgCard; _dropZone.Invalidate(); };

        _dropLabel = new Label
        {
            Text      = "⊕  Arraste arquivos PDF aqui  ou  clique para selecionar",
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontBody,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock      = DockStyle.Fill,
        };
        _dropZone.Controls.Add(_dropLabel);

        // ── ListView ───────────────────────────────────────────
        _fileList = new ListView
        {
            Dock          = DockStyle.Fill,
            View          = View.Details,
            FullRowSelect = true,
            GridLines     = false,
            MultiSelect   = false,
            BackColor     = AppTheme.BgCard,
            ForeColor     = AppTheme.TextPrimary,
            BorderStyle   = BorderStyle.None,
            Font          = AppTheme.FontMono,
            AllowDrop     = true,
            HideSelection = false,
        };
        _fileList.Columns.Add("#",       32,  HorizontalAlignment.Center);
        _fileList.Columns.Add("Arquivo", 320, HorizontalAlignment.Left);
        _fileList.Columns.Add("Págs",    52,  HorizontalAlignment.Center);
        _fileList.Columns.Add("Tamanho", 75,  HorizontalAlignment.Right);
        _fileList.Columns.Add("Caminho", 200, HorizontalAlignment.Left);
        _fileList.DragEnter       += DropZone_DragEnter;
        _fileList.DragDrop        += DropZone_DragDrop;
        _fileList.SelectedIndexChanged += (_, _) => UpdateButtonStates();
        _fileList.DoubleClick     += FileList_DoubleClick;

        // ── Side buttons ───────────────────────────────────────
        _btnPanel = new Panel
        {
            Width   = 120,
            Dock    = DockStyle.Right,
            Padding = new Padding(8, 0, 0, 0),
        };

        _btnAddFiles = CreateButton("＋ Adicionar",  AppTheme.Accent,    AppTheme.BgDeep);
        _btnRemove   = CreateButton("✕ Remover",     AppTheme.Danger,    AppTheme.BgDeep);
        _btnUp       = CreateButton("▲ Subir",       AppTheme.TextSecondary, AppTheme.BgDeep);
        _btnDown     = CreateButton("▼ Descer",      AppTheme.TextSecondary, AppTheme.BgDeep);
        _btnClearAll = CreateButton("⊘ Limpar",      AppTheme.TextMuted, AppTheme.BgDeep);

        _btnAddFiles.Click += (_, _) => OpenFileDialog();
        _btnRemove.Click   += BtnRemove_Click;
        _btnUp.Click       += BtnUp_Click;
        _btnDown.Click     += BtnDown_Click;
        _btnClearAll.Click += BtnClearAll_Click;

        var btnFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoSize      = false,
            WrapContents  = false,
            Padding       = new Padding(8, 2, 0, 0),
        };
        foreach (var b in new[] { _btnAddFiles, _btnRemove, _btnUp, _btnDown, _btnClearAll })
            btnFlow.Controls.Add(b);

        _btnPanel.Controls.Add(btnFlow);

        // ── List area (list + side buttons) ────────────────────
        var listArea = new Panel { Dock = DockStyle.Fill };
        listArea.Controls.Add(_fileList);
        listArea.Controls.Add(_btnPanel);

        // ── Output section ─────────────────────────────────────
        _outputPanel = new Panel
        {
            Height    = 44,
            Dock      = DockStyle.Bottom,
            BackColor = Color.Transparent,
            Margin    = new Padding(0, 8, 0, 0),
        };

        _lblOutput = new Label
        {
            Text      = "Salvar como:",
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontBodyBold,
            AutoSize  = true,
            Location  = new Point(0, 14),
        };

        _txtOutput = new TextBox
        {
            BackColor    = AppTheme.BgInput,
            ForeColor    = AppTheme.TextPrimary,
            BorderStyle  = BorderStyle.FixedSingle,
            Font         = AppTheme.FontMono,
            Location     = new Point(90, 10),
            Height       = 24,
            Anchor       = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
            Width        = 460,
            PlaceholderText = "Caminho de saída do PDF unificado...",
        };

        _btnBrowse = CreateButton("...", AppTheme.TextSecondary, AppTheme.BgCard, width: 36);
        _btnBrowse.Location = new Point(556, 10);
        _btnBrowse.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
        _btnBrowse.Click   += BtnBrowse_Click;

        _outputPanel.Controls.AddRange([_lblOutput, _txtOutput, _btnBrowse]);
        _outputPanel.Resize += (_, _) => _txtOutput.Width = _outputPanel.Width - 90 - 42;

        // ── Progress & status ──────────────────────────────────
        _progressBar = new ProgressBar
        {
            Height    = 4,
            Dock      = DockStyle.Bottom,
            Style     = ProgressBarStyle.Continuous,
            BackColor = AppTheme.BgCard,
            ForeColor = AppTheme.Accent,
            Value     = 0,
            Maximum   = 100,
        };

        _statusLabel = new Label
        {
            Text      = "",
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontSmall,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock      = DockStyle.Bottom,
            Height    = 20,
        };

        // ── Bottom action bar ──────────────────────────────────
        var actionBar = new Panel
        {
            Height    = 50,
            Dock      = DockStyle.Bottom,
            BackColor = AppTheme.BgPanel,
            Padding   = new Padding(0, 8, 0, 0),
        };
        actionBar.Paint += (s, e) =>
        {
            using var pen = new Pen(AppTheme.Border);
            e.Graphics.DrawLine(pen, 0, 0, actionBar.Width, 0);
        };

        _btnMerge = CreateButton("⊞  UNIFICAR PDFs", AppTheme.BgDeep, AppTheme.Accent,
                                 width: 180, height: 34, bold: true);
        _btnMerge.Location = new Point(0, 8);
        _btnMerge.Click   += BtnMerge_Click;

        _btnCancel = CreateButton("Cancelar", AppTheme.TextSecondary, AppTheme.BgDeep,
                                  width: 90, height: 34);
        _btnCancel.Location = new Point(188, 8);
        _btnCancel.Enabled  = false;
        _btnCancel.Click   += BtnCancel_Click;

        actionBar.Controls.AddRange([_btnMerge, _btnCancel]);

        // ── Assemble content ───────────────────────────────────
        content.Controls.Add(listArea);
        content.Controls.Add(actionBar);
        content.Controls.Add(_statusLabel);
        content.Controls.Add(_progressBar);
        content.Controls.Add(_outputPanel);

        // Drop zone vai acima do listArea — adicionar por cima
        var topArea = new Panel { Height = 74, Dock = DockStyle.Top };
        topArea.Controls.Add(_dropZone);
        content.Controls.Add(topArea);

        // ── Form assembly ──────────────────────────────────────
        Controls.Add(content);
        Controls.Add(_headerPanel);

        ResumeLayout(false);
        UpdateButtonStates();
        UpdateCountLabel();
    }

    // ════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════

    private static DarkButton CreateButton(string text, Color fg, Color bg,
                                           int width = 110, int height = 28, bool bold = false)
    {
        return new DarkButton
        {
            Text      = text,
            ForeColor = fg,
            BackColor = bg,
            Size      = new Size(width, height),
            Font      = bold ? AppTheme.FontBodyBold : AppTheme.FontBody,
            Margin    = new Padding(0, 0, 0, 6),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter,
        };
    }

    private void SetStatus(string msg, bool isError)
    {
        if (_statusLabel.InvokeRequired)
        {
            _statusLabel.Invoke(() => SetStatus(msg, isError));
            return;
        }
        _statusLabel.Text      = msg;
        _statusLabel.ForeColor = isError ? AppTheme.Danger : AppTheme.TextSecondary;
    }

    private void UpdateButtonStates()
    {
        bool hasItems    = _files.Count > 0;
        bool hasSelected = _fileList.SelectedIndices.Count > 0;
        int  idx         = _fileList.SelectedIndices.Count > 0 ? _fileList.SelectedIndices[0] : -1;

        _btnRemove.Enabled   = hasSelected;
        _btnUp.Enabled       = hasSelected && idx > 0;
        _btnDown.Enabled     = hasSelected && idx < _files.Count - 1;
        _btnClearAll.Enabled = hasItems;
        _btnMerge.Enabled    = hasItems;
    }

    private void UpdateCountLabel()
    {
        int total = _files.Count;
        _countLabel.Text = total == 0 ? "Nenhum arquivo"
            : total == 1 ? "1 arquivo"
            : $"{total} arquivos";
    }

    private void RefreshList()
    {
        _fileList.BeginUpdate();
        _fileList.Items.Clear();

        for (int i = 0; i < _files.Count; i++)
        {
            string path = _files[i];
            var (pages, sizeKb) = PdfService.GetInfo(path);

            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(Path.GetFileName(path));
            item.SubItems.Add(pages > 0 ? pages.ToString() : "—");
            item.SubItems.Add(sizeKb > 0 ? $"{sizeKb} KB" : "—");
            item.SubItems.Add(Path.GetDirectoryName(path) ?? "");

            item.ForeColor = AppTheme.TextPrimary;
            item.BackColor = i % 2 == 0 ? AppTheme.BgCard : AppTheme.BgPanel;
            _fileList.Items.Add(item);
        }

        _fileList.EndUpdate();
        UpdateCountLabel();
        UpdateButtonStates();
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        var newPdfs = paths
            .Where(p => p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .Where(p => !_files.Contains(p))
            .ToList();

        if (newPdfs.Count == 0)
        {
            SetStatus("Nenhum PDF novo foi adicionado (verifique se não são duplicatas).", isError: true);
            return;
        }

        _files.AddRange(newPdfs);
        RefreshList();
        SetStatus($"{newPdfs.Count} arquivo(s) adicionado(s).", isError: false);
    }

    private void OpenFileDialog()
    {
        using var dlg = new OpenFileDialog
        {
            Title       = "Selecionar arquivos PDF",
            Filter      = "Arquivos PDF (*.pdf)|*.pdf",
            Multiselect = true,
        };

        if (dlg.ShowDialog() == DialogResult.OK)
            AddFiles(dlg.FileNames);
    }

    // ════════════════════════════════════════════════════════════
    //  EVENT HANDLERS
    // ════════════════════════════════════════════════════════════

    private void DropZone_Paint(object? sender, PaintEventArgs e)
    {
        var ctrl = (Control)sender!;
        var g    = e.Graphics;
        var rect = new Rectangle(1, 1, ctrl.Width - 3, ctrl.Height - 3);

        // Dashed border
        using var pen = new Pen(AppTheme.AccentDim) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        g.DrawRectangle(pen, rect);
    }

    private void DropZone_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
            _dropZone.BackColor = AppTheme.BgHover;
            _dropZone.Invalidate();
        }
    }

    private void DropZone_DragDrop(object? sender, DragEventArgs e)
    {
        _dropZone.BackColor = AppTheme.BgCard;
        _dropZone.Invalidate();

        if (e.Data?.GetData(DataFormats.FileDrop) is string[] dropped)
            AddFiles(dropped);
    }

    private void FileList_DoubleClick(object? sender, EventArgs e)
    {
        if (_fileList.SelectedIndices.Count == 0) return;
        int idx = _fileList.SelectedIndices[0];
        if (idx >= 0 && idx < _files.Count)
        {
            try { Process.Start(new ProcessStartInfo(_files[idx]) { UseShellExecute = true }); }
            catch { SetStatus("Não foi possível abrir o arquivo.", isError: true); }
        }
    }

    private void BtnRemove_Click(object? sender, EventArgs e)
    {
        if (_fileList.SelectedIndices.Count == 0) return;
        int idx = _fileList.SelectedIndices[0];
        _files.RemoveAt(idx);
        RefreshList();
        // Reselect nearby item
        if (_fileList.Items.Count > 0)
            _fileList.Items[Math.Min(idx, _fileList.Items.Count - 1)].Selected = true;
        SetStatus("Arquivo removido.", isError: false);
    }

    private void BtnUp_Click(object? sender, EventArgs e)
    {
        if (_fileList.SelectedIndices.Count == 0) return;
        int idx = _fileList.SelectedIndices[0];
        if (idx <= 0) return;

        (_files[idx], _files[idx - 1]) = (_files[idx - 1], _files[idx]);
        RefreshList();
        _fileList.Items[idx - 1].Selected = true;
        _fileList.EnsureVisible(idx - 1);
    }

    private void BtnDown_Click(object? sender, EventArgs e)
    {
        if (_fileList.SelectedIndices.Count == 0) return;
        int idx = _fileList.SelectedIndices[0];
        if (idx >= _files.Count - 1) return;

        (_files[idx], _files[idx + 1]) = (_files[idx + 1], _files[idx]);
        RefreshList();
        _fileList.Items[idx + 1].Selected = true;
        _fileList.EnsureVisible(idx + 1);
    }

    private void BtnClearAll_Click(object? sender, EventArgs e)
    {
        var r = MessageBox.Show("Remover todos os arquivos da lista?",
                                "Confirmar", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
        if (r == DialogResult.Yes)
        {
            _files.Clear();
            RefreshList();
            SetStatus("Lista limpa.", isError: false);
        }
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog
        {
            Title            = "Salvar PDF unificado",
            Filter           = "Arquivo PDF (*.pdf)|*.pdf",
            FileName         = "unificado.pdf",
            DefaultExt       = "pdf",
            OverwritePrompt  = true,
        };

        if (dlg.ShowDialog() == DialogResult.OK)
            _txtOutput.Text = dlg.FileName;
    }

    private async void BtnMerge_Click(object? sender, EventArgs e)
    {
        // ── Validações ────────────────────────────────────────
        if (_files.Count == 0)
        {
            SetStatus("Adicione pelo menos um arquivo PDF.", isError: true);
            return;
        }

        string output = _txtOutput.Text.Trim();
        if (string.IsNullOrEmpty(output))
        {
            SetStatus("Informe o caminho de saída.", isError: true);
            _txtOutput.Focus();
            return;
        }

        if (!output.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            output += ".pdf";

        // Verificar arquivos
        var errors = PdfService.ValidateFiles(_files);
        if (errors.Count > 0)
        {
            SetStatus($"Erro: {errors[0]}", isError: true);
            MessageBox.Show(string.Join("\n", errors), "Arquivos com problema",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // ── Setup ─────────────────────────────────────────────
        _cts = new CancellationTokenSource();
        SetMerging(true);
        _progressBar.Value = 0;

        var progress = new Progress<int>(pct =>
        {
            if (_progressBar.InvokeRequired)
                _progressBar.Invoke(() => _progressBar.Value = pct);
            else
                _progressBar.Value = pct;

            SetStatus($"Processando… {pct}%", isError: false);
        });

        try
        {
            int pages = await PdfService.MergeAsync(_files, output, progress, _cts.Token);

            _progressBar.Value = 100;
            SetStatus($"✔ Concluído! {_files.Count} arquivo(s) → {pages} página(s) → {Path.GetFileName(output)}", isError: false);

            // Oferecer abrir o arquivo
            var open = MessageBox.Show(
                $"PDF unificado salvo com sucesso!\n\n{output}\n\n{pages} página(s) geradas.\n\nDeseja abrir o arquivo?",
                "Sucesso", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (open == DialogResult.Yes)
                Process.Start(new ProcessStartInfo(output) { UseShellExecute = true });
        }
        catch (OperationCanceledException)
        {
            _progressBar.Value = 0;
            SetStatus("Operação cancelada pelo usuário.", isError: false);
        }
        catch (Exception ex)
        {
            _progressBar.Value = 0;
            SetStatus($"Erro: {ex.Message}", isError: true);
            MessageBox.Show(ex.Message, "Erro ao unificar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetMerging(false);
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private void SetMerging(bool merging)
    {
        if (InvokeRequired) { Invoke(() => SetMerging(merging)); return; }

        _btnMerge.Enabled    = !merging;
        _btnCancel.Enabled   = merging;
        _btnAddFiles.Enabled = !merging;
        _btnRemove.Enabled   = !merging;
        _btnUp.Enabled       = !merging;
        _btnDown.Enabled     = !merging;
        _btnClearAll.Enabled = !merging;
    }
}

// ── Custom button with hover effect ───────────────────────────
internal sealed class DarkButton : Button
{
    private bool _hovered;

    public DarkButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize  = 1;
        FlatAppearance.BorderColor = AppTheme.Border;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true;  Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g    = e.Graphics;
        var rect = new Rectangle(0, 0, Width, Height);

        Color bg = _hovered
            ? Color.FromArgb(50, ForeColor.R, ForeColor.G, ForeColor.B)
            : BackColor;

        using (var brush = new SolidBrush(bg))
            g.FillRectangle(brush, rect);

        using (var pen = new Pen(AppTheme.Border))
            g.DrawRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1));

        TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
