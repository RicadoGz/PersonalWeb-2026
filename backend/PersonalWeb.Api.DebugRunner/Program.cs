using System.Net.Http.Json;
using System.Text.Json;

// Small console runner for inspecting the backend debug endpoint without needing
// to open the browser and manually paste the JD every time.
var baseUrl = Environment.GetEnvironmentVariable("PERSONALWEB_API_BASE_URL") ?? "http://localhost:5264";
var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "tonal-linkedin-jd.txt");
var jdText = await File.ReadAllTextAsync(fixturePath);

// Point this runner at the local backend by default.
using var client = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

Console.WriteLine($"Debugging /api/chat/debug against {baseUrl}");
Console.WriteLine();

DebugResponse? payload;

try
{
    // Hit the debug endpoint so we can inspect routing, extraction, retrieval,
    // and the final answer in one console run.
    var response = await client.PostAsJsonAsync("/api/chat/debug", new ChatRequest(jdText));

    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine($"Backend returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        return 1;
    }

    payload = await response.Content.ReadFromJsonAsync<DebugResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
}
catch (Exception ex)
{
    Console.Error.WriteLine("Could not reach backend debug endpoint.");
    Console.Error.WriteLine(ex.Message);
    return 1;
}

if (payload is null)
{
    Console.Error.WriteLine("Backend returned no debug payload.");
    return 1;
}

// Print the internal chain in the same order the backend executed it.
Console.WriteLine("=== Extraction Mode ===");
Console.WriteLine($"Mode: {payload.Mode}");
Console.WriteLine($"Router Mode: {payload.RouterMode}");
Console.WriteLine($"Router Reason: {payload.RouterReason}");
Console.WriteLine(payload.ExtractionMode);
Console.WriteLine();

Console.WriteLine("=== Extracted Requirements ===");
Console.WriteLine($"Role Title: {payload.Requirements.RoleTitle}");
Console.WriteLine($"Summary: {payload.Requirements.Summary}");
Console.WriteLine($"Required Skills: {string.Join(", ", payload.Requirements.RequiredSkills)}");
Console.WriteLine($"Preferred Skills: {string.Join(", ", payload.Requirements.PreferredSkills)}");
Console.WriteLine($"Responsibilities: {string.Join(" | ", payload.Requirements.Responsibilities)}");
Console.WriteLine($"Keywords: {string.Join(", ", payload.Requirements.Keywords)}");
Console.WriteLine();

Console.WriteLine("=== Retrieval Queries ===");
foreach (var query in payload.Queries)
{
    Console.WriteLine($"- {query}");
}
Console.WriteLine();

Console.WriteLine("=== Matched Sources ===");
foreach (var source in payload.Sources)
{
    Console.WriteLine($"- [{source.Category}] {source.Title}");
    Console.WriteLine($"  {source.Snippet}");
}
Console.WriteLine();

Console.WriteLine("=== Final Answer ===");
Console.WriteLine(payload.Answer);

return 0;

// Local contracts used only by this debug runner.
internal sealed record ChatRequest(string Message);
internal sealed record DebugResponse(
    string Input,
    string Mode,
    string RouterMode,
    string RouterReason,
    string ExtractionMode,
    JobRequirements Requirements,
    List<string> Queries,
    List<ChatSource> Sources,
    string Answer);
internal sealed record JobRequirements(
    string RoleTitle,
    string Summary,
    List<string> RequiredSkills,
    List<string> PreferredSkills,
    List<string> Responsibilities,
    List<string> Keywords);
internal sealed record ChatSource(string Title, string Snippet, string Category);
