using Xunit;
using Moq;
using FluentAssertions;
using ExecutiveBriefing.ApplicationServices.Interfaces;
using ExecutiveBriefing.ApplicationServices.Services;
using ExecutiveBriefing.Domain.Aggregates;
using ExecutiveBriefing.Domain.Repositories;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.ApplicationServices.Tests
{
    public class BriefingServiceTests
    {
        private readonly Mock<IBriefingRepository> _briefingRepositoryMock;
        private readonly Mock<IAIService> _aiServiceMock;
        private readonly Mock<IWebScraper> _webScraperMock;
        private readonly Mock<IPdfParser> _pdfParserMock;
        private readonly BriefingService _briefingService;

        public BriefingServiceTests()
        {
            _briefingRepositoryMock = new Mock<IBriefingRepository>();
            _aiServiceMock = new Mock<IAIService>();
            _webScraperMock = new Mock<IWebScraper>();
            _pdfParserMock = new Mock<IPdfParser>();

            _briefingService = new BriefingService(
                _briefingRepositoryMock.Object,
                _aiServiceMock.Object,
                _webScraperMock.Object,
                _pdfParserMock.Object
            );
        }

        [Fact]
        public async Task GenerateBriefingAsync_Should_Succeed_And_Save_When_InputsAreValid()
        {
            // Arrange
            var companyName = "Google";
            var market = "US";
            var websiteUrl = "google.com";
            var irPageUrl = "ir.google.com";

            var mockSections = new List<BriefingSection>
            {
                BriefingSection.Create("Overview", "Google overview content", 1)
            };

            _webScraperMock.Setup(w => w.ScrapeUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Mock scraped content");

            _aiServiceMock.Setup(a => a.GenerateBriefingSectionsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<SourceMaterial>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSections);

            _briefingRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<Briefing>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _briefingService.GenerateBriefingAsync(
                companyName,
                market,
                websiteUrl,
                irPageUrl,
                new List<string>(),
                new List<(string Filename, Stream Content)>(),
                CancellationToken.None
            );

            // Assert
            result.Should().NotBeNull();
            result.CompanyName.Value.Should().Be(companyName);
            result.Sections.Should().ContainSingle();
            result.Sections.First().Title.Should().Be("Overview");

            _briefingRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Briefing>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GenerateBriefingAsync_Should_Parse_Attachments_When_Uploaded()
        {
            // Arrange
            var companyName = "Microsoft";
            var mockSections = new List<BriefingSection>
            {
                BriefingSection.Create("Overview", "Microsoft overview", 1)
            };

            _pdfParserMock.Setup(p => p.ParsePdfAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Extracted PDF content");

            _aiServiceMock.Setup(a => a.GenerateBriefingSectionsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<SourceMaterial>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSections);

            using var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Act
            var result = await _briefingService.GenerateBriefingAsync(
                companyName,
                null,
                null,
                null,
                new List<string>(),
                new List<(string Filename, Stream Content)> { ("report.pdf", memoryStream) },
                CancellationToken.None
            );

            // Assert
            result.Sources.Should().ContainSingle();
            result.Sources.First().ReferenceName.Should().Be("report.pdf");
            result.Sources.First().Content.Should().Be("Extracted PDF content");
            result.Sources.First().Type.Should().Be(SourceType.Upload);

            _pdfParserMock.Verify(p => p.ParsePdfAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GenerateBriefingAsync_Should_Scrape_IRPageAndAdditionalLinks_When_Provided()
        {
            // Arrange
            var companyName = "Apple";
            var market = "US";
            var websiteUrl = "apple.com";
            var irPageUrl = "investor.apple.com";
            var additionalLinks = new List<string> { "apple.com/newsroom", "apple.com/environment" };

            var mockSections = new List<BriefingSection>
            {
                BriefingSection.Create("Financial Highlights", "Apple's financial highlights", 1)
            };

            // Set up scraping behavior for specific URLs
            _webScraperMock.Setup(w => w.ScrapeUrlAsync("apple.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync("Apple Web Content");
            _webScraperMock.Setup(w => w.ScrapeUrlAsync("investor.apple.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync("Apple IR Content");
            _webScraperMock.Setup(w => w.ScrapeUrlAsync("apple.com/newsroom", It.IsAny<CancellationToken>()))
                .ReturnsAsync("Apple News Content");
            _webScraperMock.Setup(w => w.ScrapeUrlAsync("apple.com/environment", It.IsAny<CancellationToken>()))
                .ReturnsAsync("Apple Env Content");

            IReadOnlyCollection<SourceMaterial>? capturedSources = null;

            _aiServiceMock.Setup(a => a.GenerateBriefingSectionsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<SourceMaterial>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CompanyName, string, IReadOnlyCollection<SourceMaterial>, CancellationToken>((name, mkt, src, token) => capturedSources = src)
                .ReturnsAsync(mockSections);

            // Act
            var result = await _briefingService.GenerateBriefingAsync(
                companyName,
                market,
                websiteUrl,
                irPageUrl,
                additionalLinks,
                new List<(string Filename, Stream Content)>(),
                CancellationToken.None
            );

            // Assert
            result.Sources.Should().HaveCount(4);
            
            var webSource = result.Sources.FirstOrDefault(s => s.ReferenceName == "apple.com");
            webSource.Should().NotBeNull();
            webSource!.Content.Should().Be("Apple Web Content");
            webSource.Type.Should().Be(SourceType.WebPage);

            var irSource = result.Sources.FirstOrDefault(s => s.ReferenceName == "investor.apple.com");
            irSource.Should().NotBeNull();
            irSource!.Content.Should().Be("Apple IR Content");
            irSource.Type.Should().Be(SourceType.WebPage);

            var newsSource = result.Sources.FirstOrDefault(s => s.ReferenceName == "apple.com/newsroom");
            newsSource.Should().NotBeNull();
            newsSource!.Content.Should().Be("Apple News Content");

            var envSource = result.Sources.FirstOrDefault(s => s.ReferenceName == "apple.com/environment");
            envSource.Should().NotBeNull();
            envSource!.Content.Should().Be("Apple Env Content");

            capturedSources.Should().NotBeNull();
            capturedSources.Should().BeEquivalentTo(result.Sources);
        }

        [Fact]
        public async Task GenerateBriefingAsync_Should_AutoDiscoverUrls_When_Not_Provided()
        {
            // Arrange
            var companyName = "Microsoft";
            var market = "US";

            _aiServiceMock.Setup(a => a.DiscoverCompanyUrlsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(("microsoft.com", "microsoft.com/investor"));

            _webScraperMock.Setup(w => w.ScrapeUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Mock text content");

            _aiServiceMock.Setup(a => a.GenerateBriefingSectionsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<SourceMaterial>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BriefingSection>());

            // Act
            var result = await _briefingService.GenerateBriefingAsync(
                companyName,
                market,
                null,
                null,
                new List<string>(),
                new List<(string Filename, Stream Content)>(),
                CancellationToken.None
            );

            // Assert
            result.WebsiteUrl.Should().Be("microsoft.com");
            result.IRPageUrl.Should().Be("microsoft.com/investor");

            _aiServiceMock.Verify(a => a.DiscoverCompanyUrlsAsync(
                It.Is<CompanyName>(c => c.Value == "Microsoft"),
                "US",
                It.IsAny<CancellationToken>()), Times.Once);

            _webScraperMock.Verify(w => w.ScrapeUrlAsync("microsoft.com", It.IsAny<CancellationToken>()), Times.Once);
            _webScraperMock.Verify(w => w.ScrapeUrlAsync("microsoft.com/investor", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GenerateBriefingAsync_Should_Generate_TwoPartBriefing_With_11_Sections()
        {
            // Arrange
            var companyName = "Google";
            var mockSections = new List<BriefingSection>
            {
                BriefingSection.Create("1. Visão geral da empresa", "Overview content", 1),
                BriefingSection.Create("2. Mercado e posicionamento", "Market content", 2),
                BriefingSection.Create("3. Dados financeiros", "Financials", 3),
                BriefingSection.Create("4. Resultados recentes", "Results", 4),
                BriefingSection.Create("5. Saúde atual do negócio", "Health", 5),
                BriefingSection.Create("6. Estratégia atual da empresa", "Strategy", 6),
                BriefingSection.Create("7. Notícias recentes", "News", 7),
                BriefingSection.Create("8. SWOT objetiva", "SWOT", 8),
                BriefingSection.Create("9. Organograma e liderança", "Leadership", 9),
                BriefingSection.Create("10. Foco em Tecnologia", "Technology", 10),
                BriefingSection.Create("11. Perguntas recomendadas para reunião", "Questions", 11)
            };

            _aiServiceMock.Setup(a => a.GenerateBriefingSectionsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<SourceMaterial>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSections);

            // Act
            var result = await _briefingService.GenerateBriefingAsync(
                companyName,
                null,
                null,
                null,
                new List<string>(),
                new List<(string Filename, Stream Content)>(),
                CancellationToken.None
            );

            // Assert
            result.Sections.Should().HaveCount(11);
            result.Sections.Select(s => s.Title).Should().ContainInOrder(
                "1. Visão geral da empresa",
                "2. Mercado e posicionamento",
                "3. Dados financeiros",
                "4. Resultados recentes",
                "5. Saúde atual do negócio",
                "6. Estratégia atual da empresa",
                "7. Notícias recentes",
                "8. SWOT objetiva",
                "9. Organograma e liderança",
                "10. Foco em Tecnologia",
                "11. Perguntas recomendadas para reunião"
            );
        }

        [Fact]
        public async Task GenerateBriefingAsync_Should_CrawlIRPage_And_DownloadAndParsePDFLinks_When_Discovered()
        {
            // Arrange
            var companyName = "Google";
            var irPageUrl = "https://google.com/investor";

            _aiServiceMock.Setup(a => a.DiscoverCompanyUrlsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(("https://google.com", irPageUrl));

            var mockLinks = new List<(string Text, string Url)>
            {
                ("Relatório Anual 2025", "https://google.com/reports/annual-2025.pdf"),
                ("Press Release 3Q25", "https://google.com/press/release-3q25.pdf"),
                ("Contact Us", "https://google.com/contact")
            };

            _webScraperMock.Setup(w => w.ExtractLinksAsync(irPageUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockLinks);

            // Mock downloading the PDF stream
            var pdfStream = new MemoryStream(new byte[] { 1, 2, 3 });
            _webScraperMock.Setup(w => w.DownloadFileAsync("https://google.com/reports/annual-2025.pdf", It.IsAny<CancellationToken>()))
                .ReturnsAsync(pdfStream);
            _webScraperMock.Setup(w => w.DownloadFileAsync("https://google.com/press/release-3q25.pdf", It.IsAny<CancellationToken>()))
                .ReturnsAsync(pdfStream);

            _pdfParserMock.Setup(p => p.ParsePdfAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Real parsed PDF content");

            _aiServiceMock.Setup(a => a.GenerateBriefingSectionsAsync(
                    It.IsAny<CompanyName>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<SourceMaterial>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BriefingSection>());

            // Act
            var result = await _briefingService.GenerateBriefingAsync(
                companyName,
                null,
                null,
                irPageUrl,
                new List<string>(),
                new List<(string Filename, Stream Content)>(),
                CancellationToken.None
            );

            // Assert
            _webScraperMock.Verify(w => w.ExtractLinksAsync(irPageUrl, It.IsAny<CancellationToken>()), Times.Once);
            _webScraperMock.Verify(w => w.DownloadFileAsync("https://google.com/reports/annual-2025.pdf", It.IsAny<CancellationToken>()), Times.Once);
            _webScraperMock.Verify(w => w.DownloadFileAsync("https://google.com/press/release-3q25.pdf", It.IsAny<CancellationToken>()), Times.Once);
            _pdfParserMock.Verify(p => p.ParsePdfAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            result.Sources.Should().Contain(s => s.ReferenceName == "https://google.com/reports/annual-2025.pdf" && s.Content == "Real parsed PDF content");
        }
    }
}



