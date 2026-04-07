# PersonalWeb Project Detailed Description

## 1. Project Overview

This project is a personal portfolio website for Ricardo Gao with a built-in AI assistant.

The project has two major goals:

1. Present Ricardo clearly as a candidate for AI, software, automation, and data-related roles.
2. Let visitors interact with an AI assistant that can:
   - summarize Ricardo for a recruiter
   - match Ricardo against a pasted job description
   - surface Ricardo's AI-related projects

The system is built as a full-stack application:

- Frontend: Blazor Web App
- Backend: ASP.NET Core Minimal API
- AI routing / extraction: local Ollama model
- Portfolio knowledge base: local JSON file

This means the website is not just a static portfolio. It is a portfolio plus a small agent-like assistant.

---

## 2. High-Level Product Idea

The site is designed around a simple experience:

1. A visitor lands on Ricardo's homepage.
2. They quickly understand who Ricardo is and what kind of work he does.
3. They can open a floating chatbot.
4. The chatbot can respond in three main ways:
   - recruiter summary
   - JD match
   - AI projects lookup

The AI part is meant to reduce recruiter friction:

- instead of reading a long resume manually
- instead of guessing Ricardo's fit
- instead of searching projects one by one

the visitor can ask directly and get a focused answer.

---

## 3. Project Structure

At a high level, the repository looks like this:

- `frontend/PersonalWeb.Web`
- `backend/PersonalWeb.Api`
- `backend/PersonalWeb.Api.DebugRunner`
- `backend/PersonalWeb.Api.SmokeTests`

### Frontend

The frontend is the user-facing website built with Blazor.

Important frontend files:

- [Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)
- [Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)
- [FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)
- [MainLayout.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Layout/MainLayout.razor)
- [app.css](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/wwwroot/app.css)

### Backend

The backend is a Minimal API service written in C#.

Important backend files:

- [Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)
- [ChatReAct.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/ChatReAct.cs)
- [OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs)
- [OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs)
- [PortfolioRag.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/PortfolioRag.cs)
- [info.json](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Properties/info.json)

### Debug / Test Utilities

- [Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.DebugRunner/Program.cs)
- [Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.SmokeTests/Program.cs)
- [tonal-linkedin-jd.txt](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.DebugRunner/Fixtures/tonal-linkedin-jd.txt)

---

## 4. Frontend Responsibilities

The frontend is responsible for:

- rendering the landing page
- rendering Ricardo's portfolio content
- rendering the login entry
- rendering the floating AI widget
- accepting user text input
- sending messages to the backend
- displaying backend answers

The frontend does not do the AI reasoning itself.

It does not:

- extract job requirements
- perform retrieval
- decide chunk ranking
- call the model directly for reasoning

Instead, it acts as the interface layer.

### Key Frontend Flow

1. User opens the floating chat widget.
2. User types a message.
3. [FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor) sends the message to `/api/chat`.
4. The frontend receives a `ChatResponse`.
5. The frontend displays `Answer` in the chat UI.

---

## 5. Backend Responsibilities

The backend is responsible for:

- receiving chat input
- deciding which act to use
- calling Ollama for action routing
- calling Ollama for JD extraction
- loading Ricardo's portfolio knowledge base
- chunking and embedding portfolio content
- retrieving relevant evidence
- composing the final response

This is where the main intelligence of the project lives.

---

## 6. Backend API Endpoints

The backend exposes these main endpoints:

### `GET /`

Simple health / root endpoint.

### `POST /api/auth/login`

Very lightweight login endpoint for the demo admin flow.

### `GET /api/auth/me`

Returns current simplified auth status.

### `POST /api/chat`

Main portfolio AI endpoint.

Input:

```json
{
  "message": "user message here"
}
```

Output:

```json
{
  "answer": "final assistant answer",
  "sources": [
    {
      "title": "source title",
      "snippet": "matched snippet",
      "category": "projects"
    }
  ]
}
```

### `POST /api/chat/debug`

Debug endpoint for inspecting the full internal chain.

It returns:

- selected act
- router mode
- router reason
- extraction mode
- extracted requirements
- retrieval queries
- matched sources
- final answer

This is useful for development and troubleshooting.

---

## 7. The Knowledge Base

The portfolio knowledge base lives in:

[info.json](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Properties/info.json)

This file contains Ricardo's portfolio data, including:

- experience
- projects
- education
- skills

This file acts like a lightweight local database.

The AI assistant does not read the whole website page to answer.
It reads this structured portfolio file.

That is important, because it means:

- portfolio data is controllable
- retrieval is deterministic
- the system can be improved without scraping the frontend

---

## 8. How RAG Works In This Project

This project uses a lightweight local RAG pipeline.

It is not yet a production vector database setup, but it already follows the main RAG idea:

1. Build knowledge chunks
2. Build embeddings
3. Retrieve relevant evidence
4. Generate a grounded answer

### Step 1: Load Knowledge

The backend loads `info.json`.

Code:
[PortfolioRag.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/PortfolioRag.cs)

### Step 2: Chunk Portfolio Data

The code creates chunks from:

- item summary lines
- detail bullets
- skill lists

Examples:

- a project title and timeframe summary
- one project detail bullet
- one skill category line

This gives the retriever smaller searchable units.

### Step 3: Create Lightweight Embeddings

The project currently uses a local hashed embedding approach.

This means:

- tokenize the text
- hash tokens into fixed buckets
- build a numeric vector
- normalize it

This is a lightweight stand-in for a true embedding model.

It is useful because:

- it is local
- it is fast
- it avoids needing external embedding APIs

### Step 4: Retrieve Similar Chunks

When the backend has requirements or queries, it:

- embeds the query text
- compares it against all chunk embeddings
- computes cosine-style similarity
- sorts and filters results
- deduplicates overlapping hits

### Step 5: Build Grounded Answer

The system then builds the final answer from the selected chunks.

The answer is grounded in retrieved portfolio evidence instead of being purely generated from model memory.

---

## 9. ReAct-Style Routing

The project currently uses a ReAct-like architecture.

It is not a full multi-step ReAct loop, but it is more than a simple static if/else portfolio chatbot.

The key idea is:

1. The model first decides which act to use.
2. The backend executes the corresponding method.
3. The backend returns a grounded result.

The action router lives in:

[OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs)

This file asks Ollama to select exactly one action from:

- `recruiter-summary`
- `job-match`
- `ai-projects`

### Why this matters

This means the system is not purely hand-routed anymore.

Instead:

- Ollama decides the action
- C# executes the tool

That makes the assistant more agent-like.

---

## 10. The Three Acts

The execution layer lives in:

[ChatReAct.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/ChatReAct.cs)

### Act 1: `recruiter-summary`

Purpose:

- summarize Ricardo for a recruiter
- surface the skill library
- provide a quick overall pitch

Behavior:

- no JD extraction
- no job matching
- uses portfolio skills knowledge directly

### Act 2: `job-match`

Purpose:

- evaluate fit against a pasted job description

Behavior:

- calls Ollama to extract structured job requirements
- converts those requirements into retrieval queries
- retrieves portfolio evidence
- returns a fit-oriented grounded answer

### Act 3: `ai-projects`

Purpose:

- show projects most relevant to AI-oriented roles

Behavior:

- uses AI-focused requirement seeds such as AI / RAG / LLM / automation
- retrieves project-related chunks
- returns project-focused results

---

## 11. How Job Matching Works

The `job-match` path is the most advanced path.

### Step 1: User pastes a JD

The message comes to `/api/chat`.

### Step 2: Ollama extracts requirements

Code:
[OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs)

The model tries to return structured JSON:

- `roleTitle`
- `summary`
- `requiredSkills`
- `preferredSkills`
- `responsibilities`
- `keywords`

### Step 3: Normalize and deduplicate

The extracted lists are cleaned and deduplicated before retrieval.

### Step 4: Build retrieval queries

The system combines:

- role title
- summary
- required skills
- preferred skills
- keywords
- top responsibilities

These become the retrieval queries.

### Step 5: Search Ricardo's portfolio

The queries are matched against the portfolio chunks.

### Step 6: Deduplicate retrieved evidence

Repeated or overlapping chunks are collapsed so the answer is not cluttered.

### Step 7: Build final fit answer

The backend returns:

- detected requirements
- best supporting evidence
- grounded final answer

---

## 12. Ollama Integration

Ollama is used in two places:

### 1. Action selection

[OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs)

Purpose:

- decide which act to run

### 2. Job requirement extraction

[OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs)

Purpose:

- convert messy JD text into structured requirements

The project is currently configured to use the local model:

`llama3.1:8b`

through:

`http://127.0.0.1:11434`

There is also fallback behavior if Ollama is unavailable.

---

## 13. Debugging and Inspection Workflow

The project includes a debug runner so you can inspect the full internal chain.

Important file:

[Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api.DebugRunner/Program.cs)

This runner can show:

- which act was selected
- whether the action router used Ollama or fallback
- what job requirements were extracted
- what retrieval queries were generated
- which chunks were matched
- what final answer was produced

This makes it much easier to debug prompt issues and retrieval quality.

---

## 14. Current Strengths

This project already has several strong qualities:

- clear separation between frontend and backend
- local AI routing with Ollama
- local knowledge base
- lightweight RAG pipeline
- debug visibility
- grounded answers with evidence sources
- portfolio-specific assistant instead of generic chatbot

For a personal website, this is already beyond a normal portfolio.

---

## 15. Current Limitations

The current system also has clear limitations.

### Retrieval quality

The current embedding is lightweight and hashed, not a production embedding model.

### Final answer writing

Some final answers are still composed largely in C# templates instead of a second LLM synthesis pass.

### Not a full multi-step ReAct loop

The system currently does:

- route once
- execute once
- answer

It does not yet do:

- thought
- action
- observation
- another thought
- another action

### Not multi-agent

There are no separate collaborating agents yet.

It is a single backend pipeline with routed tools.

---

## 16. What This Project Is Best Described As

The best concise description is:

> A personal portfolio website with a local AI assistant that uses Ollama-based act routing and lightweight RAG over Ricardo Gao's structured portfolio data.

If you want a more resume / interview style description:

> Built a full-stack portfolio platform with a Blazor frontend and ASP.NET Core backend, integrating a local Ollama-powered assistant that routes user intent, extracts job requirements, and performs lightweight RAG over structured portfolio data to generate grounded recruiter summaries, JD-fit analysis, and AI-project recommendations.

---

## 17. Recommended Reading Order

If you want to fully understand the code, read in this order:

1. [info.json](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Properties/info.json)
2. [PortfolioRag.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/PortfolioRag.cs)
3. [OllamaJobAnalyzer.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaJobAnalyzer.cs)
4. [OllamaActionRouter.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/OllamaActionRouter.cs)
5. [ChatReAct.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/ChatReAct.cs)
6. [Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)
7. [FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)

---

## 18. Simple End-to-End Flow

The shortest end-to-end description of the project is:

1. User types into the floating chatbot.
2. Frontend sends the message to `/api/chat`.
3. Ollama selects an act.
4. Backend executes the chosen tool path.
5. If needed, Ollama extracts JD requirements.
6. Portfolio RAG retrieves relevant evidence.
7. Backend returns `answer + sources`.
8. Frontend displays the answer.

That is the core of the project.
