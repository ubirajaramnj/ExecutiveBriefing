using Xunit;
using FluentAssertions;
using System.IO;
using ExecutiveBriefing.Infrastructure.Parsers;

namespace ExecutiveBriefing.Infrastructure.Tests
{
    public class PdfParserTests
    {
        [Fact]
        public async Task ParsePdfAsync_Should_ThrowArgumentNullException_When_StreamIsNull()
        {
            // Arrange
            var parser = new PdfParser();

            // Act
            var action = () => parser.ParsePdfAsync(null!, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ParsePdfAsync_Should_ReturnEmptyString_When_StreamIsEmpty()
        {
            // Arrange
            var parser = new PdfParser();
            using var emptyStream = new MemoryStream();

            // Act
            var result = await parser.ParsePdfAsync(emptyStream, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
