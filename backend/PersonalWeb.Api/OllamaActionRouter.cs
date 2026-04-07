using System.Net.Http.Json;
using System.Text.Json;

// Result of the action-selection step.
// Action is what the backend should execute next, and Mode tells us whether
// the choice came from Ollama or a fallback path.
public record ActionRouteResult(string Action, string Reason, string Mode);

// This file asks the local Ollama model to choose which act should run.
// It does not execute the act itself; it only returns a routing decision.
public static class OllamaActionRouter
{
    // Default to the local model currently available on this machine.
    private const string DefaultModel = "llama3.1:8b";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Guardrail: the model is only allowed to choose from a small fixed set.
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "recruiter-summary",
        "job-match",
        "ai-projects"
    };

    public static async Task<ActionRouteResult> RouteAsync(string userInput)
    {
        try
        {
            // Ask Ollama to pick the action.
            var route = await RouteWithOllamaAsync(userInput);
            var action = AllowedActions.Contains(route.Action) ? route.Action : "job-match";
            var reason = string.IsNullOrWhiteSpace(route.Reason) ? "Chosen by Ollama." : route.Reason.Trim();
            return new ActionRouteResult(action, reason, "ollama-router");
        }
        catch
        {
            // Keep the assistant alive even if Ollama is unavailable.
            return new ActionRouteResult("job-match", "Ollama router unavailable, defaulted to job-match.", "fallback-router");
        }
    }

    private static async Task<RoutePayload> RouteWithOllamaAsync(string userInput)
    {
        // The backend talks to the local Ollama HTTP server directly.
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:11434")
        };

        // Keep the router prompt narrow:
        // the model should classify intent, not answer the user.
        var prompt = $$"""
You are an action router for Ricardo's portfolio assistant.

Choose exactly one action from this list:
- recruiter-summary
- job-match
- ai-projects

Action meanings:
- recruiter-summary: the user wants a recruiter-facing overview of Ricardo, his strengths, or his skill library.
- job-match: the user pasted a job description or wants to know if Ricardo fits a role.
- ai-projects: the user wants Ricardo's AI / RAG / LLM / automation related projects.

Return valid JSON only with this exact shape:
{
  "action": "recruiter-summary | job-match | ai-projects",
  "reason": "short explanation"
}

Rules:
- If the input contains a pasted job description or asks about fit for a role, choose job-match.
- If the input asks to summarize Ricardo for a recruiter, choose recruiter-summary.
- If the input asks which projects fit AI roles or asks for AI/RAG/LLM projects, choose ai-projects.
- Pick one action only.

User input:
{{userInput}}
""";

        var request = new
        {
            // Allow override by environment variable, but use the local default otherwise.
            model = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? DefaultModel,
            prompt,
            stream = false,
            format = "json"
        };

        var response = await client.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Ollama router returned an empty response.");

        if (string.IsNullOrWhiteSpace(payload.Response))
        {
            throw new InvalidOperationException("Ollama router returned no JSON.");
        }

        var result = JsonSerializer.Deserialize<RoutePayload>(payload.Response, JsonOptions);
        return result ?? throw new InvalidOperationException("Could not parse Ollama router output.");
    }

    // Response shape returned by Ollama's generate endpoint.
    private sealed record OllamaGenerateResponse(string Response);

    // Expected JSON payload from the action router prompt.
    private sealed record RoutePayload(string Action, string Reason);
}
