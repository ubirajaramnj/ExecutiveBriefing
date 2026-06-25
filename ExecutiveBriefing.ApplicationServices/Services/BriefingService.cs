using ExecutiveBriefing.ApplicationServices.Interfaces;
using ExecutiveBriefing.Domain.Aggregates;
using ExecutiveBriefing.Domain.Repositories;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.ApplicationServices.Services
{
    public class BriefingService
    {
        private readonly IBriefingRepository _briefingRepository;
        private readonly IAIService _aiService;
        private readonly IWebScraper _webScraper;
        private readonly IPdfParser _pdfParser;

        public BriefingService(
            IBriefingRepository briefingRepository,
            IAIService aiService,
            IWebScraper webScraper,
            IPdfParser pdfParser)
        {
            _briefingRepository = briefingRepository;
            _aiService = aiService;
            _webScraper = webScraper;
            _pdfParser = pdfParser;
        }

        public async Task<Briefing> GenerateBriefingAsync(
            string companyNameStr,
            string? market,
            string? websiteUrl,
            string? irPageUrl,
            List<string> additionalLinks,
            List<(string Filename, Stream Content)> attachments,
            CancellationToken cancellationToken = default)
        {
            var companyName = new CompanyName(companyNameStr);

            // Auto-discover URLs if not provided
            if (string.IsNullOrWhiteSpace(websiteUrl) || string.IsNullOrWhiteSpace(irPageUrl))
            {
                var discovered = await _aiService.DiscoverCompanyUrlsAsync(companyName, market, cancellationToken);
                if (string.IsNullOrWhiteSpace(websiteUrl))
                {
                    websiteUrl = discovered.WebsiteUrl;
                }
                if (string.IsNullOrWhiteSpace(irPageUrl))
                {
                    irPageUrl = discovered.IrPageUrl;
                }
            }

            var briefing = Briefing.Create(companyName, market, websiteUrl, irPageUrl);

            // Gather sources
            // 1. Scrape Website if provided
            if (!string.IsNullOrWhiteSpace(websiteUrl))
            {
                var content = await _webScraper.ScrapeUrlAsync(websiteUrl, cancellationToken);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    briefing.AddSource(SourceMaterial.Create(SourceType.WebPage, websiteUrl, content));
                }
            }

            // 2. Scrape IR Page if provided
            if (!string.IsNullOrWhiteSpace(irPageUrl))
            {
                var content = await _webScraper.ScrapeUrlAsync(irPageUrl, cancellationToken);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    briefing.AddSource(SourceMaterial.Create(SourceType.WebPage, irPageUrl, content));
                }

                // Crawl and extract links of interest from IR page
                var links = await _webScraper.ExtractLinksAsync(irPageUrl, cancellationToken);
                if (links != null)
                {
                    var categories = new Dictionary<string, List<string>>
                    {
                        { "Relatório Anual", new List<string> { "relatório anual", "relatorio anual", "annual report", "10-k", "10k" } },
                        { "Release Trimestral", new List<string> { "release", "trimestral", "quarterly", "10-q", "10q", "resultado" } },
                        { "Apresentação Institucional", new List<string> { "apresentação", "apresentacao", "presentation", "institucional" } },
                        { "Transcrição de Call", new List<string> { "call", "transcrição", "transcricao", "transcript", "teleconferência", "teleconferencia" } },
                        { "Comunicado ao Mercado", new List<string> { "comunicado", "fato relevante", "announcement", "notice", "comunicado ao mercado" } }
                    };

                    foreach (var category in categories)
                    {
                        var matchedLink = links.FirstOrDefault(l =>
                            category.Value.Any(keyword => l.Text.Contains(keyword, StringComparison.OrdinalIgnoreCase) || l.Url.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        );

                        if (matchedLink.Url != null)
                        {
                            if (matchedLink.Url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || matchedLink.Url.Contains("/pdf/", StringComparison.OrdinalIgnoreCase))
                            {
                                using var pdfStream = await _webScraper.DownloadFileAsync(matchedLink.Url, cancellationToken);
                                if (pdfStream != Stream.Null)
                                {
                                    var parsedContent = await _pdfParser.ParsePdfAsync(pdfStream, cancellationToken);
                                    if (!string.IsNullOrWhiteSpace(parsedContent))
                                    {
                                        briefing.AddSource(SourceMaterial.Create(SourceType.WebPage, matchedLink.Url, parsedContent));
                                    }
                                }
                            }
                            else
                            {
                                var parsedContent = await _webScraper.ScrapeUrlAsync(matchedLink.Url, cancellationToken);
                                if (!string.IsNullOrWhiteSpace(parsedContent))
                                {
                                    briefing.AddSource(SourceMaterial.Create(SourceType.WebPage, matchedLink.Url, parsedContent));
                                }
                            }
                        }
                    }
                }
            }

            // 3. Scrape Additional Links
            if (additionalLinks != null)
            {
                foreach (var link in additionalLinks)
                {
                    if (!string.IsNullOrWhiteSpace(link))
                    {
                        var content = await _webScraper.ScrapeUrlAsync(link, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            briefing.AddSource(SourceMaterial.Create(SourceType.WebPage, link, content));
                        }
                    }
                }
            }

            // 4. Parse PDF attachments
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (attachment.Content != null)
                    {
                        var content = await _pdfParser.ParsePdfAsync(attachment.Content, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            briefing.AddSource(SourceMaterial.Create(SourceType.Upload, attachment.Filename, content));
                        }
                    }
                }
            }

            // Call AI Service to generate briefing
            var sections = await _aiService.GenerateBriefingSectionsAsync(
                companyName,
                market,
                briefing.Sources,
                cancellationToken
            );

            foreach (var section in sections)
            {
                briefing.AddSection(section);
            }

            await _briefingRepository.SaveAsync(briefing, cancellationToken);

            return briefing;
        }

        public async Task<Briefing?> GetBriefingByIdAsync(BriefingId id, CancellationToken cancellationToken = default)
        {
            return await _briefingRepository.GetByIdAsync(id, cancellationToken);
        }
    }
}
