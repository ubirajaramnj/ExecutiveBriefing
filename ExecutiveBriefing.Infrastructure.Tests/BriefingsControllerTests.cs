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
    }
}
