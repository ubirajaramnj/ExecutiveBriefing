using ExecutiveBriefing.ApplicationServices.Interfaces;

namespace ExecutiveBriefing.Infrastructure.Scrapers
{
    public class WebScraper : IWebScraper
    {
        private readonly HttpClient _httpClient;

        public WebScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ScrapeUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simple fetch for HTML
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                
                // For MVP, clean basic tags or return clean text
                // US3 will implement full HTML cleaning using HtmlAgilityPack
                return response;
            }
            catch (Exception)
            {
                // Graceful failure: return empty content if URL is unreachable or scraping fails
                return string.Empty;
            }
        }
    }
}
