# Technical Research: Briefing Generation

## Technology Decisions

### 1. PDF Parsing Library
- **Chosen**: `UglyToad.PdfPig`
- **Rationale**: It is a 100% C# library that doesn't rely on native C++ dependencies (unlike PDFsharp or pdfium wrappers), making it cross-platform compatible out-of-the-box (Linux, Docker, Windows). It parses text extraction very reliably.
- **Alternatives Considered**: 
  - `iTextSharp/iText7`: Commercial licensing restrictions (AGPL) make it unsuitable for general use.
  - `PdfSharp`: Weak text extraction support.

### 2. Web Scraping & HTML Parsing
- **Chosen**: `HtmlAgilityPack` combined with standard `HttpClient`.
- **Rationale**: `HtmlAgilityPack` is the industry standard for HTML DOM parsing in C#. It allows parsing HTML with XPath and query selectors. We will use HttpClient with custom headers (User-Agent) to retrieve page HTML.
- **Alternatives Considered**: 
  - `PuppeteerSharp`: Headless browser scraping. Rejected for MVP because it requires downloading Chromium, increasing Docker container size and complexity. Simple HTTP scraping is enough for IR pages.

### 3. AI Service (LLM) Integration
- **Chosen**: Google Gemini REST API (HttpClient)
- **Rationale**: We will invoke Gemini REST API directly using C# `HttpClient` and `System.Text.Json` serialization. This eliminates the need for heavy frameworks like `SemanticKernel` or `Microsoft.Extensions.AI`, keeping the project code lean (YAGNI).
- **Alternatives Considered**:
  - `SemanticKernel`: Too bloated for a single-use LLM call.
  - `Microsoft.Extensions.AI`: Pre-release version, less stable for immediate production.
