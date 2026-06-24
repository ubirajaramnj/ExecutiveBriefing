using ExecutiveBriefing.ApplicationServices.Interfaces;

namespace ExecutiveBriefing.Infrastructure.Parsers
{
    public class PdfParser : IPdfParser
    {
        public Task<string> ParsePdfAsync(Stream pdfStream, CancellationToken cancellationToken = default)
        {
            // Dummy implementation for US1 compilation
            // Will be fully implemented in US2
            return Task.FromResult(string.Empty);
        }
    }
}
