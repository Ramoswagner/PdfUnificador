using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfUnificador
{
    public partial class MainForm : Form
    {
        private List<string> _files = new();
        private CancellationTokenSource _cts;

        public MainForm()
        {
            InitializeComponent();
            AllowDrop = true;

            DragEnter += MainForm_DragEnter;
            DragDrop += MainForm_DragDrop;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in dropped)
            {
                if (file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    _files.Add(file);
                    listBoxFiles.Items.Add(Path.GetFileName(file));
                }
            }
        }

        private async void btnUnir_Click(object sender, EventArgs e)
        {
            if (_files.Count == 0)
            {
                MessageBox.Show("Adicione PDFs primeiro.");
                return;
            }

            using SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = "PDF_Unificado.pdf"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            _cts = new CancellationTokenSource();

            progressBar.Value = 0;
            lblStatus.Text = "Processando...";

            try
            {
                var progress = new Progress<int>(value =>
                {
                    progressBar.Value = value;
                });

                int pages = await Task.Run(() =>
                    PdfService.Merge(_files, saveDialog.FileName, progress, _cts.Token)
                );

                lblStatus.Text = $"Concluído! ({pages} páginas)";
                MessageBox.Show("PDF criado com sucesso!");
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Cancelado";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                lblStatus.Text = "Erro";
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
