using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace ExecutiveBriefing.Infrastructure.Tests
{
    public class BriefingsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BriefingsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_Briefing_Should_Return_Success_When_InputIsValid()
        {
            // Arrange
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("Google"), "companyName");
            content.Add(new StringContent("google.com"), "websiteUrl");
            content.Add(new StringContent("US"), "market");

            // Act
            var response = await _client.PostAsync("/api/briefings", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            root.GetProperty("companyName").GetProperty("value").GetString().Should().Be("Google");
            root.GetProperty("sections").GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Post_Briefing_Should_Accept_File_Uploads()
        {
            // Arrange
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("Microsoft"), "companyName");

            var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header "%PDF"
            content.Add(fileContent, "attachments", "test.pdf");

            // Act
            var response = await _client.PostAsync("/api/briefings", content);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Request failed. Status: {response.StatusCode}. Body: {jsonString}");
            }

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            root.GetProperty("companyName").GetProperty("value").GetString().Should().Be("Microsoft");
        }
    }
}
