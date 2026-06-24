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
    }
}
