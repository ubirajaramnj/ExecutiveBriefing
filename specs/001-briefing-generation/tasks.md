# Tasks: Executive Briefing Generation

**Input**: Design documents from `specs/001-briefing-generation/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD is strictly enforced; failing test tasks are defined before implementations.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Configure xUnit test projects `ExecutiveBriefing.Domain.Tests/ExecutiveBriefing.Domain.Tests.csproj` and `ExecutiveBriefing.ApplicationServices.Tests/ExecutiveBriefing.ApplicationServices.Tests.csproj` in the solution

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T002 Setup global error handling in `ExecutiveBriefing.Api/Middleware/ExceptionHandlingMiddleware.cs`
- [x] T003 Setup environment configuration for Gemini API keys in `ExecutiveBriefing.Api/Program.cs` and `appsettings.Development.json`

---

## Phase 3: User Story 1 - Basic Briefing Generation (Priority: P1) 🎯 MVP

**Goal**: User submits company name and website, and gets a structured briefing.

**Independent Test**: Verify that a POST to `/api/briefings` with "Google" and "google.com" returns a structured profile briefing.

### Tests for User Story 1 (TDD - Required) ⚠️
- [x] T004 [P] [US1] Write failing unit tests for `BriefingId` and `CompanyName` in `ExecutiveBriefing.Domain.Tests/ValueObjectsTests.cs` (Red)
- [x] T005 [P] [US1] Write failing unit tests for `Briefing` aggregate root behavior in `ExecutiveBriefing.Domain.Tests/AggregatesTests.cs` (Red)
- [x] T006 [P] [US1] Write failing unit tests for `BriefingService` basic flow in `ExecutiveBriefing.ApplicationServices.Tests/BriefingServiceTests.cs` (Red)
- [x] T007 [P] [US1] Write failing integration tests for API controller endpoint in `ExecutiveBriefing.Infrastructure.Tests/BriefingsControllerTests.cs` (Red)

### Implementation for User Story 1
- [x] T008 [P] [US1] Implement `BriefingId` in `ExecutiveBriefing.Domain/ValueObjects/BriefingId.cs` (Green)
- [x] T009 [P] [US1] Implement `CompanyName` with non-empty validation in `ExecutiveBriefing.Domain/ValueObjects/CompanyName.cs` (Green)
- [x] T010 [P] [US1] Implement `BriefingSection` in `ExecutiveBriefing.Domain/ValueObjects/BriefingSection.cs` (Green)
- [x] T011 [US1] Implement `Briefing` aggregate root behavior in `ExecutiveBriefing.Domain/Aggregates/Briefing.cs` (Green)
- [x] T012 [P] [US1] Create repository interface `IBriefingRepository` in `ExecutiveBriefing.Domain/Repositories/IBriefingRepository.cs`
- [x] T013 [US1] Implement `InMemoryBriefingRepository` in `ExecutiveBriefing.Infrastructure/Repositories/InMemoryBriefingRepository.cs`
- [x] T014 [P] [US1] Create interfaces `IAIService` and `IWebScraper` in `ExecutiveBriefing.ApplicationServices/Interfaces/`
- [x] T015 [US1] Implement `BriefingService` in `ExecutiveBriefing.ApplicationServices/Services/BriefingService.cs` (Green)
- [x] T016 [US1] Implement mock scraping in `ExecutiveBriefing.Infrastructure/Scrapers/WebScraper.cs` (Green)
- [x] T017 [US1] Implement direct Gemini API call in `ExecutiveBriefing.Infrastructure/AI/GeminiAIService.cs` (Green)
- [x] T018 [US1] Implement `BriefingsController` in `ExecutiveBriefing.Api/Controllers/BriefingsController.cs` (Green)

---

## Phase 4: User Story 2 - User Sources & Attachments (Priority: P2)

**Goal**: User uploads files (PDFs) and manual links.

**Independent Test**: Upload a PDF and verify its content is parsed and incorporated into the briefing.

### Tests for User Story 2 (TDD - Required) ⚠️
- [ ] T019 [P] [US2] Write failing unit tests for `SourceMaterial` value object in `ExecutiveBriefing.Domain.Tests/ValueObjectsTests.cs` (Red)
- [ ] T020 [P] [US2] Write failing unit tests for `PdfParser` in `ExecutiveBriefing.Infrastructure.Tests/PdfParserTests.cs` (Red)
- [ ] T021 [US2] Write failing unit tests for `BriefingService` attachment integration in `ExecutiveBriefing.ApplicationServices.Tests/BriefingServiceTests.cs` (Red)
- [ ] T022 [US2] Write failing integration tests for endpoint file upload in `ExecutiveBriefing.Infrastructure.Tests/BriefingsControllerTests.cs` (Red)

### Implementation for User Story 2
- [ ] T023 [P] [US2] Implement `SourceMaterial` in `ExecutiveBriefing.Domain/ValueObjects/SourceMaterial.cs` (Green)
- [ ] T024 [P] [US2] Create application interface `IPdfParser` in `ExecutiveBriefing.ApplicationServices/Interfaces/IPdfParser.cs`
- [ ] T025 [US2] Implement `PdfParser` using `UglyToad.PdfPig` in `ExecutiveBriefing.Infrastructure/Parsers/PdfParser.cs` (Green)
- [ ] T026 [US2] Update `BriefingService` in `ExecutiveBriefing.ApplicationServices/Services/BriefingService.cs` to process links and file streams (Green)
- [ ] T027 [US2] Update `BriefingsController` in `ExecutiveBriefing.Api/Controllers/BriefingsController.cs` to accept file uploads (Green)

---

## Phase 5: User Story 3 - Financial & Investor Relations Focus (Priority: P2)

**Goal**: Automatically scrape and prioritize investor relations material.

**Independent Test**: Verify briefing contains a "Financial Highlights" section sourced from scraped IR pages.

### Tests for User Story 3 (TDD - Required) ⚠️
- [ ] T028 [P] [US3] Write failing unit tests for `WebScraper` (IR scraping logic) in `ExecutiveBriefing.Infrastructure.Tests/WebScraperTests.cs` (Red)
- [ ] T029 [US3] Write failing unit tests for `BriefingService` IR flow in `ExecutiveBriefing.ApplicationServices.Tests/BriefingServiceTests.cs` (Red)

### Implementation for User Story 3
- [ ] T030 [US3] Implement real HTML scraper for IR links using `HtmlAgilityPack` in `ExecutiveBriefing.Infrastructure/Scrapers/WebScraper.cs` (Green)
- [ ] T031 [US3] Update `BriefingService` in `ExecutiveBriefing.ApplicationServices/Services/BriefingService.cs` to feed scraped IR texts to prompt context (Green)

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: General cleanup, quickstart validation, and standards checking

- [ ] T032 Run quickstart validation scenario in `specs/001-briefing-generation/quickstart.md`
- [ ] T033 Verify zero code drift and compliance with Clean Code and DDD rules in the constitution
- [ ] T034 Complete OpenAPI Swagger annotations in `ExecutiveBriefing.Api/Controllers/BriefingsController.cs`

---

## Dependencies & Execution Order

### Phase Dependencies
- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup. Blocks all user stories.
- **User Stories (Phases 3-5)**: Depend on Foundational completion. Can run sequentially or in parallel.
- **Polish (Phase 6)**: Depends on all user stories completion.

### Parallel Opportunities
- All Setup tasks marked [P] can run in parallel.
- Test tasks marked [P] can run in parallel within their respective phases.
- Value object models marked [P] can be created in parallel.

---

## Parallel Example: User Story 1
```bash
# Run unit tests in parallel
dotnet test --filter Category=Unit
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Complete Setup and Foundational.
2. Complete Phase 3 (US1).
3. Run test cases and verify MVP output.
4. Add attachments and IR scraping sequentially as Phase 4 and 5.
