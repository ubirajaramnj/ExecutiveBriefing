using System.Text;
using UglyToad.PdfPig;
using ExecutiveBriefing.ApplicationServices.Interfaces;

namespace ExecutiveBriefing.Infrastructure.Parsers
{
    public class PdfParser : IPdfParser
    {
        public async Task<string> ParsePdfAsync(Stream pdfStream, CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
            {
                throw new ArgumentNullException(nameof(pdfStream));
            }

            if (pdfStream.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                // Reset stream position if seekable
                if (pdfStream.CanSeek && pdfStream.Position > 0)
                {
                    pdfStream.Position = 0;
                }

                return await Task.Run(() =>
                {
                    using var document = PdfDocument.Open(pdfStream);
                    var textBuilder = new StringBuilder();

                    foreach (var page in document.GetPages())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        textBuilder.AppendLine(page.Text);
                    }

                    return textBuilder.ToString().Trim();
                }, cancellationToken);
            }
            catch (Exception)
            {
                // Return empty or throw based on preference, return empty to degrade gracefully
                return string.Empty;
            }
        }
    }
}
