using BaseApp.Application.Common.Interfaces;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BaseApp.Infrastructure.Files;

public class PdfFileBuilder : IPdfFileBuilder
{
    public byte[] Merge(IEnumerable<byte[]> pdfs)
    {
        using var salida = new PdfDocument();

        foreach (var pdf in pdfs)
        {
            using var entrada = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);

            foreach (var pagina in entrada.Pages)
            {
                salida.AddPage(pagina);
            }
        }

        using var stream = new MemoryStream();
        salida.Save(stream, false);

        return stream.ToArray();
    }
}
