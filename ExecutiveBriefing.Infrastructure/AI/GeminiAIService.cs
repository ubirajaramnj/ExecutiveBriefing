using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ExecutiveBriefing.ApplicationServices.Interfaces;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.Infrastructure.AI
{
    public class GeminiAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;

        public GeminiAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
            _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
            _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";
        }

        public async Task<List<BriefingSection>> GenerateBriefingSectionsAsync(
            CompanyName companyName,
            string? market,
            IReadOnlyCollection<SourceMaterial> sources,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                // Fallback mock if API key is not configured (e.g. during local tests)
                return GetMockSections(companyName.Value);
            }

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Generate a professional executive briefing for the company '{companyName.Value}' (Market: {market ?? "Global"}).");
            promptBuilder.AppendLine("Provide the output in valid markdown with the following sections: Overview, Financial Highlights, Strategic Outlook, Recent News.");
            promptBuilder.AppendLine("Base your analysis on the following source materials retrieved:");

            foreach (var source in sources)
            {
                promptBuilder.AppendLine($"--- SOURCE: {source.ReferenceName} ({source.Type}) ---");
                promptBuilder.AppendLine(source.Content.Length > 2000 ? source.Content[..2000] : source.Content);
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptBuilder.ToString() }
                        }
                    }
                }
            };

            var url = $"{_baseUrl}{_model}:generateContent?key={_apiKey}";
            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, requestContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                
                var textResponse = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(textResponse))
                {
                    return GetMockSections(companyName.Value);
                }

                return ParseMarkdownToSections(textResponse);
            }
            catch (Exception)
            {
                return GetMockSections(companyName.Value);
            }
        }

        private static List<BriefingSection> ParseMarkdownToSections(string markdown)
        {
            var sections = new List<BriefingSection>();
            var lines = markdown.Split('\n');
            string? currentTitle = null;
            var currentContent = new StringBuilder();
            int order = 1;

            foreach (var line in lines)
            {
                if (line.StartsWith("## ") || line.StartsWith("### "))
                {
                    if (currentTitle != null && currentContent.Length > 0)
                    {
                        sections.Add(BriefingSection.Create(currentTitle, currentContent.ToString().Trim(), order++));
                        currentContent.Clear();
                    }
                    currentTitle = line.Replace("##", "").Replace("###", "").Trim();
                }
                else
                {
                    currentContent.AppendLine(line);
                }
            }

            if (currentTitle != null && currentContent.Length > 0)
            {
                sections.Add(BriefingSection.Create(currentTitle, currentContent.ToString().Trim(), order));
            }

            if (sections.Count == 0)
            {
                sections.Add(BriefingSection.Create("Executive Summary", markdown, 1));
            }

            return sections;
        }

        private static List<BriefingSection> GetMockSections(string companyName)
        {
            return new List<BriefingSection>
            {
                BriefingSection.Create("Overview", $"### Overview of {companyName}\nThis is a mock overview for {companyName}.", 1),
                BriefingSection.Create("Financial Highlights", $"### Financials\nMock financial highlights for {companyName}.", 2),
                BriefingSection.Create("Strategic Outlook", $"### Strategy\nMock strategic direction.", 3)
            };
        }
    }
}
