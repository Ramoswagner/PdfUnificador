using System;
using System.Collections.Generic;
using System.Threading;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfUnificador
{
    public static class PdfService
    {
        public static int Merge(
            IReadOnlyList<string> sourceFiles,
            string outputPath,
            IProgress<int>? progress = null,
            CancellationToken ct = default)
        {
            using var output = new PdfDocument();

            int totalPages = 0;

            for (int i = 0; i < sourceFiles.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                using var doc = PdfReader.Open(sourceFiles[i], PdfDocumentOpenMode.Import);

                foreach (PdfPage page in doc.Pages)
                {
                    ct.ThrowIfCancellationRequested();
                    output.AddPage(page);
                    totalPages++;
                }

                int pct = (int)Math.Round((i + 1) / (double)sourceFiles.Count * 100);
                progress?.Report(pct);
            }

            output.Save(outputPath);
            return totalPages;
        }
    }
}
