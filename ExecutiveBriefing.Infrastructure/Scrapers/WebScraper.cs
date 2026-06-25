using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ExecutiveBriefing.ApplicationServices.Interfaces;

namespace ExecutiveBriefing.Infrastructure.Scrapers
{
    public class WebScraper : IWebScraper
    {
        private readonly HttpClient _httpClient;

        public WebScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Configure default headers to avoid blockages
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<string> ScrapeUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            try
            {
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                if (string.IsNullOrWhiteSpace(response))
                {
                    return string.Empty;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                var root = doc.DocumentNode;
                if (root == null)
                {
                    return string.Empty;
                }

                // Remove unwanted nodes (scripts, styles, headers, footers, navs)
                var nodesToRemove = root.SelectNodes("//script|//style|//header|//footer|//nav|//noscript|//iframe");
                if (nodesToRemove != null)
                {
                    foreach (var node in nodesToRemove)
                    {
                        node.Remove();
                    }
                }

                // Extract remaining text
                var rawText = root.InnerText;
                if (string.IsNullOrWhiteSpace(rawText))
                {
                    return string.Empty;
                }

                // Decode HTML entities (e.g. &nbsp;, &amp;)
                var decodedText = HtmlEntity.DeEntitize(rawText);

                // Normalize whitespace (reduce multiple spaces/newlines to a single space/newline)
                var cleanText = Regex.Replace(decodedText, @"\s+", " ").Trim();

                return cleanText;
            }
            catch (Exception)
            {
                // Graceful degradation: return empty string if scraping fails
                return string.Empty;
            }
        }

        public async Task<List<(string Text, string Url)>> ExtractLinksAsync(string url, CancellationToken cancellationToken = default)
        {
            var links = new List<(string Text, string Url)>();
            if (string.IsNullOrWhiteSpace(url))
            {
                return links;
            }

            try
            {
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                if (string.IsNullOrWhiteSpace(response))
                {
                    return links;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                var root = doc.DocumentNode;
                if (root == null)
                {
                    return links;
                }

                var baseUri = new Uri(url);
                var anchorNodes = root.SelectNodes("//a[@href]");
                if (anchorNodes != null)
                {
                    foreach (var node in anchorNodes)
                    {
                        var href = node.GetAttributeValue("href", string.Empty);
                        if (string.IsNullOrWhiteSpace(href) || href.StartsWith('#') || href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string absoluteUrl;
                        if (Uri.TryCreate(baseUri, href, out var resolvedUri))
                        {
                            absoluteUrl = resolvedUri.AbsoluteUri;
                        }
                        else
                        {
                            absoluteUrl = href;
                        }

                        var text = HtmlEntity.DeEntitize(node.InnerText ?? string.Empty).Trim();
                        text = Regex.Replace(text, @"\s+", " ").Trim();

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            links.Add((text, absoluteUrl));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Graceful degradation: return empty list on failure
            }

            return links;
        }

        public async Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Stream.Null;
            }

            try
            {
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception)
            {
                return Stream.Null;
            }
        }
    }
}
