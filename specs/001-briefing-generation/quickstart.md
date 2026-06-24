# Quickstart Validation Guide: Executive Briefing

This guide outlines the steps to verify that the Executive Briefing Generation feature works correctly.

## Prerequisites
- .NET 10.0 SDK installed
- Local environment (or Docker)

## Step-by-Step Verification

### 1. Compile the Project
From the repository root directory, run:
```bash
dotnet build
```

### 2. Run the API Project
Start the API server in development mode:
```bash
dotnet run --project ExecutiveBriefing.Api --launch-profile http
```
The server will start listening at `http://localhost:5198`.

### 3. API Test Request
To generate a new briefing, send a `POST` request to the `/api/briefings` endpoint.

#### Sample Request (Using curl)
```bash
curl -X POST "http://localhost:5198/api/briefings" \
  -H "Content-Type: multipart/form-data" \
  -F "companyName=Google" \
  -F "websiteUrl=google.com" \
  -F "market=US" \
  -F "attachments=@sample_report.pdf"
```

#### Expected Outcome
The server will return `200 OK` or `201 Created` with a JSON payload containing the generated briefing sections (markdown format) and the sources referenced.
```json
{
  "id": "a5d6e24f-...",
  "companyName": "Google",
  "createdAt": "2026-06-24T19:00:00Z",
  "sections": [
    {
      "title": "Company Overview",
      "content": "### Google Overview\nGoogle is a global leader in technology..."
    }
  ],
  "sources": [
    {
      "type": "Upload",
      "referenceName": "sample_report.pdf"
    }
  ]
}
```
