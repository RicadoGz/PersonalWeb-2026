using System.Net.Http.Json;

var baseUrl = Environment.GetEnvironmentVariable("PERSONALWEB_API_BASE_URL") ?? "http://localhost:5264";
var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "tonal-linkedin-jd.txt");
var jdText = await File.ReadAllTextAsync(fixturePath);

using var client = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

Console.WriteLine($"Testing /api/chat against {baseUrl}");

HttpResponseMessage response;

try
{
    response = await client.PostAsJsonAsync("/api/chat", new ChatRequest(jdText));
}
catch (Exception ex)
{
    Console.Error.WriteLine("FAIL: Could not reach backend /api/chat.");
    Console.Error.WriteLine(ex.Message);
    return 1;
}

if (!response.IsSuccessStatusCode)
{
    Console.Error.WriteLine($"FAIL: Backend returned {(int)response.StatusCode} {response.ReasonPhrase}.");
    return 1;
}

var payload = await response.Content.ReadFromJsonAsync<ChatResponse>();

if (payload is null)
{
    Console.Error.WriteLine("FAIL: Backend returned an empty JSON payload.");
    return 1;
}

var failures = new List<string>();

if (string.IsNullOrWhiteSpace(payload.Answer))
{
    failures.Add("Answer is empty.");
}

if (payload.Sources is null || payload.Sources.Count == 0)
{
    failures.Add("Sources are empty.");
}

var lowerAnswer = (payload.Answer ?? string.Empty).ToLowerInvariant();
var noisyTerms = new[]
{
    "show more options",
    "responses managed off linkedin",
    "clicked apply",
    "resume match",
    "simplify",
    "logo"
};

foreach (var term in noisyTerms)
{
    if (lowerAnswer.Contains(term))
    {
        failures.Add($"Answer still contains noisy job-board text: '{term}'.");
    }
}

var expectedSignals = new[]
{
    "software",
    "automation",
    "ai",
    "test",
    "engineering"
};

if (!expectedSignals.Any(lowerAnswer.Contains))
{
    failures.Add("Answer does not mention any expected high-signal requirement words.");
}

if (failures.Count > 0)
{
    Console.Error.WriteLine("FAIL");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"- {failure}");
    }

    Console.Error.WriteLine();
    Console.Error.WriteLine("Raw answer:");
    Console.Error.WriteLine(payload.Answer);
    return 1;
}

Console.WriteLine("PASS");
Console.WriteLine();
Console.WriteLine("Answer:");
Console.WriteLine(payload.Answer);
Console.WriteLine();
Console.WriteLine("Sources:");
foreach (var source in payload.Sources!)
{
    Console.WriteLine($"- [{source.Category}] {source.Title}");
}

return 0;

internal sealed record ChatRequest(string Message);
internal sealed record ChatResponse(string Answer, List<ChatSource>? Sources);
internal sealed record ChatSource(string Title, string Snippet, string Category);
