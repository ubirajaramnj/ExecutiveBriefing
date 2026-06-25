using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExecutiveBriefing.Infrastructure.Scrapers;
using Moq;
using Moq.Protected;

namespace ExecutiveBriefing.Infrastructure.Tests
{
    public class WebScraperTests
    {
        [Fact]
        public async Task ScrapeUrlAsync_Should_ReturnCleanText_And_IgnoreScriptsAndStyles()
        {
            // Arrange
            var html = @"
                <html>
                <head>
                    <style>body { color: red; }</style>
                    <script>console.log('hello');</script>
                </head>
                <body>
                    <header>Navigation Header</header>
                    <nav>Menu Items</nav>
                    <main>
                        <h1>Company Overview</h1>
                        <p>Google is a technology company.</p>
                    </main>
                    <footer>Copyright 2026</footer>
                </body>
                </html>";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(html),
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var scraper = new WebScraper(httpClient);

            // Act
            var result = await scraper.ScrapeUrlAsync("https://example.com/ir", CancellationToken.None);

            // Assert
            result.Should().Contain("Company Overview");
            result.Should().Contain("Google is a technology company.");
            result.Should().NotContain("console.log");
            result.Should().NotContain("Navigation Header");
            result.Should().NotContain("Copyright 2026");
        }

        [Fact]
        public async Task ExtractLinksAsync_Should_ReturnAbsoluteAndRelativeLinks_WithCleanText()
        {
            // Arrange
            var html = @"
                <html>
                <body>
                    <a href='/reports/annual-2025.pdf'>Relatório Anual 2025</a>
                    <a href='https://example.com/press/release-3q25'>Press Release 3Q25</a>
                    <a href='#anchor'>Skip to main</a>
                    <a>No href link</a>
                </body>
                </html>";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(html),
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var scraper = new WebScraper(httpClient);

            // Act
            var links = await scraper.ExtractLinksAsync("https://example.com/ir", CancellationToken.None);

            // Assert
            links.Should().HaveCount(2);
            links.Should().Contain(l => l.Text == "Relatório Anual 2025" && l.Url == "https://example.com/reports/annual-2025.pdf");
            links.Should().Contain(l => l.Text == "Press Release 3Q25" && l.Url == "https://example.com/press/release-3q25");
        }
    }
}
