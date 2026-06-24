namespace ExecutiveBriefing.ApplicationServices.Interfaces
{
    public interface IWebScraper
    {
        Task<string> ScrapeUrlAsync(string url, CancellationToken cancellationToken = default);
    }
}
