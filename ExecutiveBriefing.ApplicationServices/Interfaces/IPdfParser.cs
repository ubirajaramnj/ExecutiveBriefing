namespace ExecutiveBriefing.ApplicationServices.Interfaces
{
    public interface IPdfParser
    {
        Task<string> ParsePdfAsync(Stream pdfStream, CancellationToken cancellationToken = default);
    }
}
