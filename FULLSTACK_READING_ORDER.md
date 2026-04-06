# Full-Stack Reading Order

This document is the recommended reading order for the current project.

It is written for someone who already has:

- basic C# knowledge
- basic Django knowledge

So the goal is not to teach programming from zero.
The goal is to help you quickly map this codebase into concepts you already know.

---

## 1. What This Project Is

Right now this project has two main parts:

- `frontend/PersonalWeb.Web`
  Blazor frontend
- `backend/PersonalWeb.Api`
  ASP.NET Core backend

The site currently does 3 main things:

1. shows Ricardo's portfolio homepage
2. provides a login flow
3. provides a portfolio chatbot / first-pass RAG-style fit assistant

---

## 2. Best Overall Reading Strategy

Do **not** read file-by-file in filesystem order.

Instead, read in this order:

1. understand what the user sees
2. understand how frontend pages are composed
3. understand frontend interaction
4. understand backend endpoints
5. understand how frontend calls backend
6. understand styling last

That is the fastest way to fully understand this project.

---

## 3. Recommended Reading Order

## Step 1. Homepage First

Read:
[Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)

Why:

- this is the public homepage
- it shows the actual portfolio structure
- it tells you what the site is trying to communicate

What to learn here:

- hero section
- project section
- experience section
- contact / skills section

Django analogy:

- this is like your main template content for `/`
- but in Blazor it is a component page instead of a Django HTML template

---

## Step 2. Understand the Small Hero Interaction

Read:
[HelloRotator.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/HelloRotator.razor)

Why:

- this is a small self-contained interactive Blazor component
- it helps you understand how stateful UI works in this project

What to learn here:

- `Timer`
- component state
- `StateHasChanged`
- `Dispose`

C# analogy:

- this is just a small C# class-backed UI component with state and lifecycle

---

## Step 3. Understand the Shared Page Frame

Read:
[MainLayout.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Layout/MainLayout.razor)

Why:

- this wraps all pages
- this gives you the global layout
- this mounts the floating chatbot widget

What to learn here:

- `@Body`
- top bar
- where the floating widget is injected

Django analogy:

- this is close to a base template layout
- `@Body` is similar to a content block being inserted

---

## Step 4. Understand the Chat Widget

Read:
[FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)

Why:

- this is the most important interactive frontend component
- it sends user messages
- it renders assistant responses
- it connects to the backend `/api/chat`

What to learn here:

- message list state
- `draftMessage`
- submit flow
- `HttpClientFactory`
- `PostAsJsonAsync("/api/chat", ...)`

Django analogy:

- this is similar to JS in a Django template that sends AJAX to an API endpoint
- the difference is the UI logic lives inside a Blazor component

---

## Step 5. Understand the Login Page

Read:
[Login.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Login.razor)

Then:
[LoginFormCard.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/LoginFormCard.razor)

Why:

- `Login.razor` is the page
- `LoginFormCard.razor` is the reusable login form logic

What to learn:

- routing vs reusable UI
- form binding
- how frontend calls `/api/auth/login`

Django analogy:

- `Login.razor` is like the page view/template
- `LoginFormCard.razor` is like the reusable form partial plus form handling logic together

---

## Step 6. Understand Frontend Startup

Read:
[Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)

Why:

- this tells you how the frontend starts
- this registers services
- this registers the named backend HTTP client

What to focus on:

- `AddRazorComponents()`
- `AddInteractiveServerComponents()`
- `AddHttpClient("BackendApi", ...)`
- `MapRazorComponents<App>()`

Django analogy:

- this is not exactly `settings.py`, but it plays a similar startup/configuration role

---

## Step 7. Understand Frontend Routing

Read:
[Routes.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Routes.razor)

Then:
[App.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/App.razor)

Why:

- `Routes.razor` explains how `/` and `/login` are routed
- `App.razor` explains the app shell and route outlet

Django analogy:

- `Routes.razor` is conceptually closest to `urls.py`
- `App.razor` is closer to the document shell / app root

---

## Step 8. Understand Backend Entry

Read:
[Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)

Why:

- all backend endpoints are currently here
- auth and chat API are both defined here

What to focus on first:

- `MapGet("/")`
- `MapPost("/api/auth/login", ...)`
- `MapGet("/api/auth/me", ...)`
- `MapPost("/api/chat", ...)`

Django analogy:

- this file currently combines parts of `urls.py`, views, and lightweight service logic

Important note:

- right now the backend is intentionally minimal
- later you would usually split this into controllers/services/modules

---

## Step 9. Understand Chat Backend Logic

Still in:
[Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)

Focus specifically on:

- `ChatRequest`
- `ChatResponse`
- `ChatSource`
- `KnowledgeItem`
- `PortfolioRag`

Why:

- this is the first-pass RAG-style logic
- this is where retrieval happens
- this is where the portfolio knowledge base is stored

What to understand:

1. user sends a question
2. backend tokenizes it
3. backend compares it against knowledge items
4. top matches are selected
5. response text is built from matched items
6. answer is sent back to frontend

Django analogy:

- this is like a view plus a simple service layer in one file
- `PortfolioRag` is the equivalent of a tiny retrieval service

---

## Step 10. Understand Styling Last

Read:
[app.css](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/wwwroot/app.css)

Why last:

- it is large
- it only becomes easy once you already understand component structure

Best way to read it:

search these class groups in order:

1. `.intro-stage`
2. `.hello-rotator`
3. `.resume-section`
4. `.project-grid`
5. `.floating-agent`
6. `.login-page-shell`

That will let you map style blocks to actual components you already know.

---

## 4. Fastest Useful Path

If you want the shortest path to understanding:

1. [Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)
2. [FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)
3. [LoginFormCard.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/LoginFormCard.razor)
4. [frontend Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)
5. [backend Program.cs](/Users/gz/Desktop/coding/personalWeb/backend/PersonalWeb.Api/Program.cs)
6. [app.css](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/wwwroot/app.css)

That path is enough to understand:

- what the site is
- how frontend works
- how frontend talks to backend
- how the chatbot works

---

## 5. If You Think In Django Terms

Here is the quickest mapping:

- `frontend Program.cs`
  startup / app wiring
- `Routes.razor`
  similar to `urls.py`
- `Home.razor`, `Login.razor`
  similar to page templates + page logic together
- `MainLayout.razor`
  similar to base template
- `backend Program.cs`
  like a compressed `urls.py + views.py + simple services`
- `app.css`
  shared frontend styling

The main mindset shift is:

In Django you often split:

- route
- view
- template
- JS

In this Blazor frontend, much more of that is kept together in a single `.razor` component.

---

## 6. If You Think In C# Terms

Use this mindset:

- `.razor` files are UI components with markup + C# logic together
- `@code` is just component logic
- `@inject` is dependency injection into the component
- `Program.cs` registers services and app startup
- `HttpClientFactory` is your API client factory

So this is still very much a C# application.
It just happens that UI and logic are co-located in components.

---

## 7. What To Read Next With Me

Best next guided walkthrough options:

1. `Home.razor`
   if you want to understand page structure
2. `FloatingAgentWidget.razor`
   if you want to understand interaction and chat flow
3. `backend Program.cs`
   if you want to understand the current RAG-style backend logic

If your goal is full understanding, the best next file is:

[FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)

because it sits exactly at the boundary between:

- frontend UI
- frontend state
- backend API calls
- chatbot behavior
