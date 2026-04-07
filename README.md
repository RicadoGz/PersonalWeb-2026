# PersonalWeb

Ricardo Gao's full-stack portfolio website with a built-in AI assistant.

This project is not just a static portfolio page.
It combines:

- a Blazor frontend
- an ASP.NET Core backend
- a local Ollama-powered action router
- a lightweight portfolio RAG pipeline over structured JSON data

The assistant can currently do three main things:

1. summarize Ricardo for a recruiter
2. match Ricardo to a pasted job description
3. surface projects that fit AI roles

---

## Stack

### Frontend

- Blazor Web App
- Razor components
- `HttpClientFactory` for backend API calls

### Backend

- ASP.NET Core Minimal API
- C#
- local Ollama model: `llama3.1:8b`
- local JSON knowledge base
- lightweight retrieval / RAG layer

---

## Project Structure

### Frontend

- [frontend/PersonalWeb.Web/Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)
- [frontend/PersonalWeb.Web/Components/Pages/Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)
- [frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)
- [frontend/PersonalWeb.Web/wwwroot/app.css](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/wwwroot/app.css)

### Backend

- [backend/PersonalWeb.Api/Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)
- [backend/PersonalWeb.Api/ChatReAct.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/ChatReAct.cs)
- [backend/PersonalWeb.Api/OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs)
- [backend/PersonalWeb.Api/OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs)
- [backend/PersonalWeb.Api/PortfolioRag.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/PortfolioRag.cs)
- [backend/PersonalWeb.Api/Properties/info.json](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Properties/info.json)

### Debug / Test Utilities

- [backend/PersonalWeb.Api.DebugRunner/Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.DebugRunner/Program.cs)
- [backend/PersonalWeb.Api.DebugRunner/Fixtures/tonal-linkedin-jd.txt](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.DebugRunner/Fixtures/tonal-linkedin-jd.txt)
- [backend/PersonalWeb.Api.SmokeTests/Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.SmokeTests/Program.cs)

---

## How It Works

### Frontend -> Backend

The main frontend/backend connection happens here:

- [frontend/PersonalWeb.Web/Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)
- [frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)
- [backend/PersonalWeb.Api/Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)

The frontend registers a named backend client:

```csharp
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5264");
});
```

Then the floating chat widget sends:

```csharp
await client.PostAsJsonAsync("/api/chat", new ChatRequest(userMessage));
```

The backend receives that at:

```csharp
app.MapPost("/api/chat", async (ChatRequest request) => ...)
```

---

## Chat Architecture

The backend chat flow is:

1. frontend sends a message to `/api/chat`
2. backend passes the message to [ChatReAct.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/ChatReAct.cs)
3. [OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs) asks Ollama to choose an act
4. backend runs the corresponding act
5. if needed, [OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs) extracts structured JD requirements
6. [PortfolioRag.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/PortfolioRag.cs) retrieves relevant portfolio evidence
7. backend returns `answer + sources`
8. frontend displays the answer in the chat widget

### Current Acts

#### `recruiter-summary`

- returns Ricardo's skill library / recruiter-facing summary
- does not run JD extraction

#### `job-match`

- used when the router thinks the message is a JD or role-fit request
- runs JD extraction
- runs RAG retrieval
- returns a fit-oriented answer

#### `ai-projects`

- retrieves AI / RAG / LLM / automation-related projects
- skips JD extraction

---

## RAG Layer

The knowledge base is stored in:

[backend/PersonalWeb.Api/Properties/info.json](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Properties/info.json)

The current RAG pipeline does this:

1. load structured portfolio data from `info.json`
2. split it into chunks
3. create lightweight local embeddings using hashed token buckets
4. compare requirement queries against chunk vectors
5. rank and deduplicate matches
6. build a grounded answer from matched chunks

This is a lightweight local RAG implementation.
It is not a production vector database setup yet, but it is already a real retrieval pipeline.

---

## Ollama Usage

This project is set up to use your local Ollama instance.

Current default model:

```text
llama3.1:8b
```

Current default Ollama endpoint:

```text
http://127.0.0.1:11434
```

Ollama is currently used for:

- action routing
- JD requirement extraction

If Ollama is unavailable, the backend falls back to simpler local logic instead of crashing.

---

## Run The Project

### 1. Start the backend

```bash
dotnet run --project backend/PersonalWeb.Api
```

Backend default URL:

```text
http://localhost:5264
```

### 2. Start the frontend

```bash
dotnet run --project frontend/PersonalWeb.Web
```

Frontend default URL:

```text
http://localhost:5173
```

### 3. Open the site

```text
http://localhost:5173
```

---

## Debug The Chat Pipeline

If you want to inspect the internal chain, use the debug runner.

### Start the backend first

```bash
dotnet run --project backend/PersonalWeb.Api
```

### Then run:

```bash
dotnet run --project backend/PersonalWeb.Api.DebugRunner
```

This will print:

- selected act
- router mode
- router reason
- extraction mode
- extracted requirements
- retrieval queries
- matched sources
- final answer

This is the best way to inspect whether the system is routing and retrieving correctly.

---

## Smoke Test

There is also a simple smoke test project:

```bash
dotnet run --project backend/PersonalWeb.Api.SmokeTests
```

It checks basic chat endpoint behavior against the Tonal JD fixture.

---

## Build Commands

### Backend

```bash
dotnet build backend/PersonalWeb.Api/PersonalWeb.Api.csproj
```

### Frontend

```bash
dotnet build frontend/PersonalWeb.Web/PersonalWeb.Web.csproj
```

### Debug Runner

```bash
dotnet build backend/PersonalWeb.Api.DebugRunner/PersonalWeb.Api.DebugRunner.csproj
```

---

## Recommended Reading Order

If you want to understand the project quickly, read in this order:

1. [frontend/PersonalWeb.Web/Components/Pages/Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)
2. [frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)
3. [frontend/PersonalWeb.Web/Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)
4. [backend/PersonalWeb.Api/Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)
5. [backend/PersonalWeb.Api/OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs)
6. [backend/PersonalWeb.Api/ChatReAct.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/ChatReAct.cs)
7. [backend/PersonalWeb.Api/OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs)
8. [backend/PersonalWeb.Api/PortfolioRag.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/PortfolioRag.cs)
9. [backend/PersonalWeb.Api/Properties/info.json](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Properties/info.json)

There is also a fuller reading guide here:

[FULLSTACK_READING_ORDER.md](/Users/gz/Desktop/coding/personalWeb/FULLSTACK_READING_ORDER.md)

---

## Current Status

What this project already has:

- portfolio landing page
- floating AI assistant
- frontend/backend integration
- local Ollama action router
- JD extraction
- lightweight RAG over portfolio data
- debug endpoint
- debug runner
- smoke test utility

What this project does not yet have:

- production auth
- persistent chat memory
- vector database
- second-pass LLM answer synthesis
- full multi-step ReAct loop

So the best way to describe it right now is:

> A personal portfolio website with a Blazor frontend and ASP.NET Core backend, using local Ollama-based action routing and a lightweight portfolio RAG pipeline to power recruiter summaries, JD-fit analysis, and AI-project search.
