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
            if (_apiKey == "YOUR_GEMINI_API_KEY")
            {
                _apiKey = string.Empty;
            }
            _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
            _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";
        }

        public async Task<List<BriefingSection>> GenerateBriefingSectionsAsync(
            CompanyName companyName,
            string? market,
            IReadOnlyCollection<SourceMaterial> sources,
            CancellationToken cancellationToken = default)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Generate a professional executive briefing for the company '{companyName.Value}' (Market: {market ?? "Global"}).");
            promptBuilder.AppendLine("Provide the output in valid markdown with two main parts and the following sections exactly as headings (e.g. ## 1. Visão geral da empresa):");
            promptBuilder.AppendLine("# PARTE 1 — BRIEFING EXECUTIVO DA EMPRESA");
            promptBuilder.AppendLine("## 1. Visão geral da empresa");
            promptBuilder.AppendLine("## 2. Mercado e posicionamento");
            promptBuilder.AppendLine("## 3. Dados financeiros");
            promptBuilder.AppendLine("## 4. Resultados recentes");
            promptBuilder.AppendLine("## 5. Saúde atual do negócio");
            promptBuilder.AppendLine("## 6. Estratégia atual da empresa");
            promptBuilder.AppendLine("## 7. Notícias recentes");
            promptBuilder.AppendLine("## 8. SWOT objetiva");
            promptBuilder.AppendLine("# PARTE 2 — LIDERANÇA, TECNOLOGIA E PRIORIDADES DIGITAIS");
            promptBuilder.AppendLine("## 9. Organograma e liderança");
            promptBuilder.AppendLine("  * Identificar principais executivos da empresa (CEO, CFO, COO, CIO, CTO, CDO, CISO, líderes de tecnologia, dados, digital, operações e transformação). Mapear o organograma executivo, descrever responsabilidades, missões e prioridades públicas dessas lideranças, informando fontes utilizadas.");
            promptBuilder.AppendLine("## 10. Foco em Tecnologia");
            promptBuilder.AppendLine("  * Aprofundar especificamente nas áreas de Tecnologia, Digital, Dados, Segurança, Inovação e Transformação. Gerar bullets sobre: planejamento estratégico de Tecnologia, prioridades atuais da área de Tecnologia, estratégia digital, projetos de transformação em andamento, uso de IA, automação, cloud, dados, analytics, cibersegurança, modernização de sistemas e últimas vitórias.");
            promptBuilder.AppendLine("## 11. Perguntas recomendadas para reunião");
            promptBuilder.AppendLine("  * Formular de 3 a 5 perguntas altamente qualificadas e personalizadas para reunião com executivos, focadas em desafios, oportunidades, tecnologia e negócios identificados no briefing.");
            promptBuilder.AppendLine("Base your analysis on the following source materials retrieved:");

            foreach (var source in sources)
            {
                promptBuilder.AppendLine($"--- SOURCE: {source.ReferenceName} ({source.Type}) ---");
                promptBuilder.AppendLine(source.Content.Length > 2000 ? source.Content[..2000] : source.Content);
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                var localResult = await GenerateViaOllamaAsync(promptBuilder.ToString(), cancellationToken);
                if (localResult != null)
                {
                    return ParseMarkdownToSections(localResult);
                }
                return GetMockSections(companyName.Value);
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

        public async Task<(string? WebsiteUrl, string? IrPageUrl)> DiscoverCompanyUrlsAsync(
            CompanyName companyName,
            string? market,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                var localResult = await DiscoverViaOllamaAsync(companyName, market, cancellationToken);
                if (localResult != null)
                {
                    return localResult.Value;
                }
                if (companyName.Value.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    return ("https://www.google.com", "https://abc.xyz/investor");
                }
                return ($"https://www.{companyName.Value.ToLower().Replace(" ", "")}.com", null);
            }

            var prompt = $"Identify the official main website URL and the Investor Relations page URL for the company '{companyName.Value}' (Market: {market ?? "Global"}). " +
                         "Respond ONLY with a valid raw JSON object matching this schema: { \"websiteUrl\": \"https://...\", \"irPageUrl\": \"https://...\" }. " +
                         "Do not include markdown code block formatting like ```json or any other text.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                tools = new[]
                {
                    new { google_search = new { } }
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
                var textResponse = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(textResponse))
                {
                    return (null, null);
                }

                // Clean markdown code blocks if the model included them anyway
                var cleanedText = textResponse.Trim();
                if (cleanedText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                {
                    cleanedText = cleanedText["```json".Length..].Trim();
                }
                if (cleanedText.StartsWith("```", StringComparison.OrdinalIgnoreCase))
                {
                    cleanedText = cleanedText["```".Length..].Trim();
                }
                if (cleanedText.EndsWith("```"))
                {
                    cleanedText = cleanedText[..^"```".Length].Trim();
                }

                using var parsedResponse = JsonDocument.Parse(cleanedText);
                var root = parsedResponse.RootElement;
                string? website = null;
                string? ir = null;

                if (root.TryGetProperty("websiteUrl", out var webProp))
                {
                    website = webProp.GetString();
                }
                if (root.TryGetProperty("irPageUrl", out var irProp))
                {
                    ir = irProp.GetString();
                }

                return (string.IsNullOrWhiteSpace(website) ? null : website, string.IsNullOrWhiteSpace(ir) ? null : ir);
            }
            catch (Exception)
            {
                return (null, null);
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
                    if (currentTitle != null)
                    {
                        var contentStr = currentContent.ToString().Trim();
                        if (string.IsNullOrWhiteSpace(contentStr))
                        {
                            contentStr = "Informações não disponíveis.";
                        }
                        sections.Add(BriefingSection.Create(currentTitle, contentStr, order++));
                        currentContent.Clear();
                    }
                    currentTitle = line.Replace("##", "").Replace("###", "").Trim();
                }
                else
                {
                    currentContent.AppendLine(line);
                }
            }

            if (currentTitle != null)
            {
                var contentStr = currentContent.ToString().Trim();
                if (string.IsNullOrWhiteSpace(contentStr))
                {
                    contentStr = "Informações não disponíveis.";
                }
                sections.Add(BriefingSection.Create(currentTitle, contentStr, order));
            }

            if (sections.Count == 0)
            {
                var contentStr = markdown.Trim();
                if (string.IsNullOrWhiteSpace(contentStr))
                {
                    contentStr = "Informações não disponíveis.";
                }
                sections.Add(BriefingSection.Create("Executive Summary", contentStr, 1));
            }

            return sections;
        }

        private static List<BriefingSection> GetMockSections(string companyName)
        {
            return new List<BriefingSection>
            {
                BriefingSection.Create("1. Visão geral da empresa", $"### Visão geral da {companyName}\nMock Visão geral for {companyName}.", 1),
                BriefingSection.Create("2. Mercado e posicionamento", "### Mercado\nMock Mercado e posicionamento.", 2),
                BriefingSection.Create("3. Dados financeiros", "### Financeiro\nMock Dados financeiros.", 3),
                BriefingSection.Create("4. Resultados recentes", "### Resultados\nMock Resultados recentes.", 4),
                BriefingSection.Create("5. Saúde atual do negócio", "### Saúde\nMock Saúde atual do negócio.", 5),
                BriefingSection.Create("6. Estratégia atual da empresa", "### Estratégia\nMock Estratégia atual da empresa.", 6),
                BriefingSection.Create("7. Notícias recentes", "### Notícias\nMock Notícias recentes.", 7),
                BriefingSection.Create("8. SWOT objetiva", "### SWOT\nMock SWOT objetiva.", 8),
                BriefingSection.Create("9. Organograma e liderança", "### Liderança\nMock Organograma e liderança.", 9),
                BriefingSection.Create("10. Foco em Tecnologia", "### Tecnologia\nMock Foco em Tecnologia.", 10),
                BriefingSection.Create("11. Perguntas recomendadas para reunião", "### Perguntas\nMock Perguntas recomendadas para reunião.", 11)
            };
        }

        private async Task<string?> GenerateViaOllamaAsync(string prompt, CancellationToken cancellationToken)
        {
            try
            {
                var requestBody = new
                {
                    model = "gemma2:2b",
                    prompt = prompt,
                    stream = false
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", requestContent, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(jsonString);
                return doc.RootElement.GetProperty("response").GetString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<(string? WebsiteUrl, string? IrPageUrl)?> DiscoverViaOllamaAsync(CompanyName companyName, string? market, CancellationToken cancellationToken)
        {
            try
            {
                var prompt = $"Identify the official main website URL and the Investor Relations page URL for the company '{companyName.Value}' (Market: {market ?? "Global"}). " +
                             "Respond ONLY with a valid raw JSON object matching this schema: { \"websiteUrl\": \"https://...\", \"irPageUrl\": \"https://...\" }. " +
                             "Do not include markdown code block formatting like ```json or any other text.";

                var requestBody = new
                {
                    model = "gemma2:2b",
                    prompt = prompt,
                    stream = false,
                    options = new { temperature = 0.0 }
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", requestContent, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(jsonString);
                var textResponse = doc.RootElement.GetProperty("response").GetString();

                if (string.IsNullOrWhiteSpace(textResponse))
                {
                    return null;
                }

                var cleanedText = textResponse.Trim();
                if (cleanedText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                {
                    cleanedText = cleanedText["```json".Length..].Trim();
                }
                if (cleanedText.StartsWith("```", StringComparison.OrdinalIgnoreCase))
                {
                    cleanedText = cleanedText["```".Length..].Trim();
                }
                if (cleanedText.EndsWith("```"))
                {
                    cleanedText = cleanedText[..^"```".Length].Trim();
                }

                using var parsedResponse = JsonDocument.Parse(cleanedText);
                var root = parsedResponse.RootElement;
                string? website = null;
                string? ir = null;

                if (root.TryGetProperty("websiteUrl", out var webProp))
                {
                    website = webProp.GetString();
                }
                if (root.TryGetProperty("irPageUrl", out var irProp))
                {
                    ir = irProp.GetString();
                }

                return (string.IsNullOrWhiteSpace(website) ? null : website, string.IsNullOrWhiteSpace(ir) ? null : ir);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

