// Minimal API startup entry for the backend service.
// This file stays intentionally small: it wires routes together and delegates
// actual chat behavior to dedicated files such as ChatReAct, Ollama router,
// and the portfolio RAG layer.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Keep local development traffic simple and consistent.
app.UseHttpsRedirection();

// Simple root endpoint so we can quickly confirm the API is alive.
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Hello from ASP.NET Core"
    });
});

// Lightweight demo login endpoint.
// This is not a production auth system; it just supports the current UI flow.
app.MapPost("/api/auth/login", (LoginRequest request) =>
{
    if (request.Username == "demo-admin" && request.Password == "123456")
    {
        return Results.Ok(new
        {
            authenticated = true,
            username = request.Username
        });
    }

    return Results.BadRequest(new
    {
        authenticated = false,
        error = "Invalid username or password"
    });
});

// Matching lightweight auth status endpoint used by the frontend.
app.MapGet("/api/auth/me", () =>
{
    return Results.Ok(new
    {
        authenticated = false
    });
});

// Main chat endpoint used by the floating assistant on the frontend.
// The route only validates the input and then hands control to ChatReAct.
app.MapPost("/api/chat", async (ChatRequest request) =>
{
    var question = (request.Message ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(question))
    {
        return Results.BadRequest(new
        {
            error = "Message is required"
        });
    }

    // ChatReAct decides which act to run and returns a normalized ChatResponse.
    var response = await ChatReAct.RunAsync(question);
    return Results.Ok(response);
});

// Debug endpoint for development.
// This exposes the internal pipeline so we can inspect:
// - which act was selected
// - whether routing came from Ollama or fallback logic
// - what JD requirements were extracted
// - what retrieval queries and sources were used
app.MapPost("/api/chat/debug", async (ChatRequest request) =>
{
    var question = (request.Message ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(question))
    {
        return Results.BadRequest(new
        {
            error = "Message is required"
        });
    }

    var debug = await ChatReAct.RunDebugAsync(question);
    return Results.Ok(debug);
});

app.Run();

// Basic request/response contracts used by the auth and chat endpoints.
public record LoginRequest(string Username, string Password);

public record ChatRequest(string Message);

public record ChatResponse(string Answer, List<ChatSource> Sources);

public record ChatSource(string Title, string Snippet, string Category);

public record ChatDebugResponse(
    string Input,
    string Mode,
    string RouterMode,
    string RouterReason,
    string ExtractionMode,
    JobRequirements Requirements,
    List<string> Queries,
    List<ChatSource> Sources,
    string Answer);
