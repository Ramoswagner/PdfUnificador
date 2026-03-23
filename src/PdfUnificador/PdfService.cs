using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfUnificador;

/// <summary>
/// Serviço responsável por toda operação com PDFs.
/// </summary>
internal static class PdfService
{
    /// <summary>
    /// Unifica uma lista de arquivos PDF em um único arquivo de saída.
    /// </summary>
    /// <param name="sourceFiles">Caminhos dos PDFs de entrada (em ordem).</param>
    /// <param name="outputPath">Caminho completo do arquivo de saída.</param>
    /// <param name="progress">Callback de progresso (0-100).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Quantidade total de páginas geradas.</returns>
    public static async Task<int> MergeAsync(
        IReadOnlyList<string> sourceFiles,
        string outputPath,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        if (sourceFiles.Count == 0)
            throw new ArgumentException("Nenhum arquivo fornecido.", nameof(sourceFiles));

        return await Task.Run(() =>
        {
            using var output = new PdfDocument();
            output.Info.Title   = "PDF Unificado";
            output.Info.Creator = "PDF Unificador — Wagner Ramos";
            output.Info.Author  = Environment.UserName;

            int totalPages = 0;

            for (int i = 0; i < sourceFiles.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                string path = sourceFiles[i];

                if (!File.Exists(path))
                    throw new FileNotFoundException($"Arquivo não encontrado: {path}");

                using var doc = PdfReader.Open(path, PdfDocumentOpenMode.Import);

                foreach (PdfPage page in doc.Pages)
                {
                    ct.ThrowIfCancellationRequested();
                    output.AddPage(page);
                    totalPages++;
                }

                int pct = (int)Math.Round((i + 1) / (double)sourceFiles.Count * 100);
                progress?.Report(pct);
            }

            // Garante que o diretório de saída existe
            string? dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            output.Save(outputPath);
            return totalPages;

        }, ct);
    }

    /// <summary>
    /// Retorna metadados básicos de um PDF (página e tamanho em KB).
    /// </summary>
    public static (int pages, long sizeKb) GetInfo(string filePath)
    {
        if (!File.Exists(filePath)) return (0, 0);

        long sizeKb = new FileInfo(filePath).Length / 1024;

        using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
        return (doc.PageCount, sizeKb);
    }

    /// <summary>
    /// Valida se todos os arquivos são PDFs legíveis.
    /// </summary>
    public static List<string> ValidateFiles(IEnumerable<string> paths)
    {
        var errors = new List<string>();

        foreach (string path in paths)
        {
            if (!File.Exists(path))
            {
                errors.Add($"Não encontrado: {Path.GetFileName(path)}");
                continue;
            }

            if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Não é PDF: {Path.GetFileName(path)}");
                continue;
            }

            try
            {
                using var doc = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                _ = doc.PageCount; // força leitura
            }
            catch
            {
                errors.Add($"PDF corrompido ou protegido: {Path.GetFileName(path)}");
            }
        }

        return errors;
    }
}
