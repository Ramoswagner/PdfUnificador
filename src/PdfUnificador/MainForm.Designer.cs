namespace PdfUnificador
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnUnir;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnUnir = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.listBoxFiles = new System.Windows.Forms.ListBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // listBoxFiles
            this.listBoxFiles.FormattingEnabled = true;
            this.listBoxFiles.Location = new System.Drawing.Point(12, 12);
            this.listBoxFiles.Size = new System.Drawing.Size(360, 160);

            // btnUnir
            this.btnUnir.Text = "Unir PDFs";
            this.btnUnir.Location = new System.Drawing.Point(12, 180);
            this.btnUnir.Click += new System.EventHandler(this.btnUnir_Click);

            // btnCancelar
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Location = new System.Drawing.Point(120, 180);
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // progressBar
            this.progressBar.Location = new System.Drawing.Point(12, 220);
            this.progressBar.Size = new System.Drawing.Size(360, 23);

            // lblStatus
            this.lblStatus.Text = "Pronto";
            this.lblStatus.Location = new System.Drawing.Point(12, 250);

            // MainForm
            this.ClientSize = new System.Drawing.Size(384, 281);
            this.Controls.Add(this.listBoxFiles);
            this.Controls.Add(this.btnUnir);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblStatus);
            this.Text = "PDF Unificador";

            this.ResumeLayout(false);
        }
    }
}
