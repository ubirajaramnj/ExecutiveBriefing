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
