# Implementation Plan: Executive Briefing Generation

**Branch**: `001-briefing-generation` | **Date**: 2026-06-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/001-briefing-generation/spec.md`

## Summary
The goal is to implement an ASP.NET Core 10 Web API service that automatically generates complete executive briefings about client companies. It will accept user inputs (company name, optional websites/links, and attachments) and coordinate text extraction from PDFs, web scraping of IR/news sites, and a summarization/analysis phase using an AI model. All implementation must adhere strictly to DDD, TDD, YAGNI, and Clean Architecture principles.

## Technical Context

**Language/Version**: C# / .NET 10.0

**Primary Dependencies**: 
- `UglyToad.PdfPig` (pure C# lightweight PDF parsing)
- `HtmlAgilityPack` (HTML parsing for web scraping)
- `Microsoft.Extensions.Http` (resilient HttpClient)

**Storage**: Stateless memory storage (`InMemoryBriefingRepository`) for MVP to adhere strictly to YAGNI.

**Testing**: xUnit + Moq + FluentAssertions

**Target Platform**: Cross-platform (Docker, Windows, Linux)

**Project Type**: ASP.NET Core Web API + Clean Architecture Libraries

**Performance Goals**: File parsing under 5s; Web scraping under 15s; Complete briefing generation under 3 minutes (constrained by AI LLM response latency).

**Constraints**: Pure Domain model with no external dependencies (no library dependencies in `ExecutiveBriefing.Domain`).

**Scale/Scope**: Support files up to 15MB.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Rule / Principle | Verification Method | Status |
|------------------|---------------------|--------|
| **Clean Architecture Layering** | Verify project dependencies: Domain has zero external dependencies; ApplicationServices depends only on Domain; Infrastructure and Api depend on ApplicationServices and Domain. | Pass |
| **DDD Enforcement** | Ensure all domain classes have behavior-rich methods, no public setters on Entities, Value Objects are immutable, and Repository interfaces are in the Domain layer. | Pass |
| **TDD Requirement** | Write xUnit tests in `ExecutiveBriefing.Domain.Tests` and `ExecutiveBriefing.ApplicationServices.Tests` before writing implementation code. | Pass |
| **YAGNI (Simplicity)** | No CQRS, MediatR, or SQL databases will be introduced. Core Web API controllers will invoke Application Services directly. | Pass |

## Project Structure

### Documentation (this feature)

```text
specs/001-briefing-generation/
├── plan.md              # This file
├── research.md          # Research findings
├── data-model.md        # Domain entities and value objects design
├── quickstart.md        # Run/Validation guide
├── contracts/           # API Contract definition (Swagger/OpenAPI JSON)
└── tasks.md             # Task breakdown checklist
```

### Source Code (repository root)

We will use the existing clean architecture structure:
```text
ExecutiveBriefing.Domain/
├── Aggregates/
│   └── Briefing/
│       ├── Briefing.cs (Aggregate Root)
│       └── BriefingId.cs (Value Object)
├── ValueObjects/
│   ├── CompanyName.cs
│   ├── SourceMaterial.cs
│   └── BriefingSection.cs
└── Repositories/
    └── IBriefingRepository.cs

ExecutiveBriefing.ApplicationServices/
├── Interfaces/
│   ├── IAIService.cs
│   ├── IPdfParser.cs
│   └── IWebScraper.cs
└── Services/
    └── BriefingService.cs (Application Service)

ExecutiveBriefing.Infrastructure/
├── Parsers/
│   └── PdfParser.cs (PdfPig implementation)
├── Scrapers/
│   └── WebScraper.cs (HttpClient + HtmlAgilityPack implementation)
├── AI/
│   └── GeminiAIService.cs (LLM Integration using HttpClient)
└── Repositories/
    └── InMemoryBriefingRepository.cs

ExecutiveBriefing.Api/
├── Controllers/
│   └── BriefingsController.cs
└── Program.cs
```

## Complexity Tracking

No violations of the Constitution. The architecture is kept simple and follows the layered structure defined during project initialization.
