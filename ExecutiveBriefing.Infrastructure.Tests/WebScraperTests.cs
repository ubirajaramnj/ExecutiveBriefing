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
    }
}
