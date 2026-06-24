# Feature Specification: Executive Briefing Generation

**Feature Branch**: `001-briefing-generation`

**Created**: 2026-06-24

**Status**: Draft

**Input**: User description: "O objetivo do app é gerar automaticamente um briefing executivo completo sobre uma empresa cliente a partir do nome da empresa informado pelo usuário..."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Briefing Generation (Priority: P1)
As an executive/consultant, I want to input a company's name and website to get a quick, structured summary of what the company does and its basic profile, so that I can prepare for an initial client call.

**Why this priority**: It is the core value proposition of the product. Without a basic generation from a company name/website, the app has no MVP value.

**Independent Test**: Can be fully tested by entering "Google" and "google.com" and verifying that a structured markdown summary profile is returned.

**Acceptance Scenarios**:
1. **Given** the user is on the main entry screen, **When** they input the name "Acme Corp" and website "acme.com" and submit, **Then** the system successfully generates and displays a briefing containing a Company Profile, Core Business Description, and High-Level Industry segment.
2. **Given** the user submits only a company name without a website, **When** the system runs, **Then** it attempts to auto-discover the website and generates the profile, or prompts for website confirmation if multiple targets are found.

---

### User Story 2 - User Sources & Attachments (Priority: P2)
As a business analyst, I want to upload files (PDFs, reports, earnings transcripts) and add manual web links when requesting a briefing, so that the generated briefing includes private or highly specific information not easily found publicly.

**Why this priority**: Increases the accuracy and customization of the briefing beyond generic public web data.

**Independent Test**: Can be tested by uploading a sample PDF report and verifying that key terms from the PDF are highlighted and summarized in the generated briefing.

**Acceptance Scenarios**:
1. **Given** the user is requesting a briefing, **When** they upload a PDF document and submit, **Then** the system parses the PDF contents, integrates them with the public web search results, and references the upload as a source.
2. **Given** the user inputs two additional custom links, **When** the system processes the request, **Then** it scrapes the content of those specific links and prioritizes their findings in the final briefing.

---

### User Story 3 - Financial & Investor Relations Focus (Priority: P2)
As a financial analyst, I want the briefing to prioritize the Investor Relations page, annual/quarterly reports, and earnings transcripts so that the financial health and strategic outlook of the company are detailed and accurate.

**Why this priority**: Essential for business context; financial history and official investor relations material are the highest-value sources for executive briefings.

**Independent Test**: Verify that the briefing contains a dedicated "Financial Highlights & IR Strategy" section containing latest earnings/revenue figures.

**Acceptance Scenarios**:
1. **Given** a company has a public Investor Relations page, **When** the briefing is generated, **Then** the system automatically scrapes and prioritizes the latest annual (10-K/relatório anual) and quarterly (10-Q/release de resultados) reports.

---

### Edge Cases
- **No public data found**: If the company has no public website or IR page, the system must degrade gracefully, using only user-supplied attachments and links, and displaying a warning that public data was unavailable.
- **Corrupt or unreadable attachment**: If an uploaded PDF is password-protected or corrupt, the system must notify the user immediately and allow them to proceed with the remaining inputs or upload a new file.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST accept user inputs: Company Name, Country/Market (optional), Website URL (optional), IR Page URL (optional), Additional Source Links (optional).
- **FR-002**: System MUST support file upload for attachments (PDF, TXT, DOCX, presentation files).
- **FR-003**: System MUST execute a prioritized search/scraping strategy targeting (in order): User uploads, manual links, official IR page, annual/quarterly reports, earnings call transcripts, news.
- **FR-004**: System MUST parse and extract text content from uploaded PDF and text documents.
- **FR-005**: System MUST aggregate and summarize public information gathered from web scraping.
- **FR-006**: System MUST compile a structured, readable Executive Briefing document containing: Company Overview, Financial Performance, Strategic Outlook, Recent News/Events, and Sources List.
- **FR-007**: System MUST cite and list all sources used (e.g., specific URLs, uploaded files) at the end of the briefing.

### Key Entities
- **BriefingRequest**: Represents the user's input data (Company name, website, custom links, attached file references).
- **ExecutiveBriefing**: The generated output document containing summarized sections (Overview, Financials, Strategy, News) and citations.
- **SourceMaterial**: Represents a single piece of retrieved information (a web page content, scraped document, or uploaded attachment).

## Success Criteria *(mandatory)*

### Measurable Outcomes
- **SC-001**: Briefings must be generated in under 3 minutes when processing up to 3 web sources and 1 attachment.
- **SC-002**: Generated briefings must include a "Sources & Citations" section listing 100% of the files uploaded by the user and pages successfully scraped.
- **SC-003**: System successfully extracts text from at least 95% of standard readable (non-scanned/non-secured) PDF uploads.

## Assumptions
- The system has access to the internet for web searching and scraping.
- Optical Character Recognition (OCR) for scanned PDFs is out of scope for the initial version.
- User files upload size is capped at 15MB per file.
