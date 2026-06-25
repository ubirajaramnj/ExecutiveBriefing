namespace ExecutiveBriefing.ApplicationServices.Interfaces
{
    public interface IWebScraper
    {
        Task<string> ScrapeUrlAsync(string url, CancellationToken cancellationToken = default);
        Task<List<(string Text, string Url)>> ExtractLinksAsync(string url, CancellationToken cancellationToken = default);
        Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken = default);
    }
}
