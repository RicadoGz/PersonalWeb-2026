using System.Net.Http.Json;
using System.Text.Json;

// Structured requirement shape used by the job-match act.
// The rest of the retrieval pipeline depends on this normalized format.
public record JobRequirements(
    string RoleTitle,
    string Summary,
    List<string> RequiredSkills,
    List<string> PreferredSkills,
    List<string> Responsibilities,
    List<string> Keywords);

// Mode lets us see whether extraction came from Ollama or from the local fallback logic.
public record JobAnalysisResult(string Mode, JobRequirements Requirements);

// This file is responsible for turning a noisy job description into
// a cleaner structured requirement object.
public static class OllamaJobAnalyzer
{
    private const string DefaultModel = "llama3.1:8b";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<JobRequirements> AnalyzeAsync(string jdText)
    {
        // Convenience wrapper used by the normal chat path when we do not need
        // to expose debug metadata.
        var result = await AnalyzeWithDebugAsync(jdText);
        return result.Requirements;
    }

    public static async Task<JobAnalysisResult> AnalyzeWithDebugAsync(string jdText)
    {
        try
        {
            // Preferred path: use the local model to extract requirements.
            var extracted = await ExtractWithOllamaAsync(jdText);
            return new JobAnalysisResult("ollama", Normalize(extracted));
        }
        catch
        {
            // Fallback path: avoid total failure if the model is down or returns bad JSON.
            return new JobAnalysisResult("fallback", Normalize(FallbackExtract(jdText)));
        }
    }

    private static async Task<JobRequirements> ExtractWithOllamaAsync(string jdText)
    {
        // Direct local Ollama HTTP call.
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:11434")
        };

        // The prompt is intentionally strict because LinkedIn-style pasted text
        // includes a lot of irrelevant platform chrome.
        var prompt = $$"""
You extract real hiring requirements from noisy job posting text.

The input may include job-board chrome, LinkedIn UI text, buttons, engagement stats, branding, or navigation text.
Ignore all platform noise such as:
- logo
- share
- show more options
- apply / save
- responses managed off LinkedIn
- hybrid / remote badges
- people clicked apply
- simplify / match score / resume match

Only extract real job information from sections like:
- overview
- what you'll do
- responsibilities
- qualifications
- who you are
- requirements
- preferred qualifications

Return valid JSON only with this exact shape:
{
  "roleTitle": "string",
  "summary": "string",
  "requiredSkills": ["string"],
  "preferredSkills": ["string"],
  "responsibilities": ["string"],
  "keywords": ["string"]
}

Rules:
- Do not include UI words, company marketing fluff, or page controls.
- Keep skills short, normalized, and technical/professional.
- Prefer concrete requirements like Python, SQL, automation, testing, communication, AI tools, APIs.
- Responsibilities should be short action statements, not copied paragraphs.
- Keywords should be high-signal matching terms only.
- Remove duplicates and near-duplicates.
- If the role title is unclear, infer the most likely role from the actual JD content.

Job posting text:
{{jdText}}
""";

        var request = new
        {
            // Environment variable can override the local default model when needed.
            model = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? DefaultModel,
            prompt,
            stream = false,
            format = "json"
        };

        var response = await client.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Ollama returned an empty response.");

        if (string.IsNullOrWhiteSpace(payload.Response))
        {
            throw new InvalidOperationException("Ollama returned no extracted JSON.");
        }

        var result = JsonSerializer.Deserialize<JobRequirements>(payload.Response, JsonOptions);
        return result ?? throw new InvalidOperationException("Could not parse Ollama extraction output.");
    }

    private static JobRequirements FallbackExtract(string jdText)
    {
        // This is intentionally simple. It is not meant to be "smart", only to
        // keep the pipeline usable when Ollama extraction fails.
        var lines = jdText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var roleTitle = lines.FirstOrDefault() ?? "Unknown role";
        var summary = string.Join(" ", lines.Take(3));

        var keywords = jdText
            .ToLowerInvariant()
            .Split([' ', ',', '.', ';', ':', '/', '\\', '(', ')', '[', ']', '{', '}', '-', '_', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => term.Length > 2)
            .Where(term => !StopWords.Contains(term))
            .Distinct()
            .Take(12)
            .ToList();

        return new JobRequirements(
            RoleTitle: roleTitle,
            Summary: summary,
            RequiredSkills: keywords.Take(6).ToList(),
            PreferredSkills: keywords.Skip(6).Take(4).ToList(),
            Responsibilities: lines.Skip(1).Take(4).ToList(),
            Keywords: keywords);
    }

    private static JobRequirements Normalize(JobRequirements requirements)
    {
        // Normalize every list before retrieval so we do not query on repeated
        // or obviously messy terms.
        return new JobRequirements(
            RoleTitle: CleanText(requirements.RoleTitle),
            Summary: CleanText(requirements.Summary),
            RequiredSkills: Deduplicate(requirements.RequiredSkills),
            PreferredSkills: Deduplicate(requirements.PreferredSkills),
            Responsibilities: Deduplicate(requirements.Responsibilities),
            Keywords: Deduplicate(requirements.Keywords));
    }

    private static List<string> Deduplicate(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var value in values)
        {
            var cleaned = CleanText(value);
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                continue;
            }

            var normalizedKey = NormalizeKey(cleaned);
            if (!seen.Add(normalizedKey))
            {
                continue;
            }

            result.Add(cleaned);
        }

        return result;
    }

    private static string CleanText(string? value)
    {
        // Keep the cleaning step simple and predictable.
        return (value ?? string.Empty).Trim();
    }

    private static string NormalizeKey(string value)
    {
        // Used only for deduplication, so punctuation and case are stripped.
        return new string(value
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    // Response wrapper from Ollama's generate API.
    private sealed record OllamaGenerateResponse(string Response);

    // Fallback extraction ignores many common low-signal words so we do not
    // immediately fill the requirement lists with platform noise.
    private static readonly HashSet<string> StopWords =
    [
        "the", "and", "for", "with", "that", "this", "from", "you", "your", "our", "are",
        "will", "all", "has", "have", "who", "using", "into", "able", "about", "role",
        "team", "work", "job", "candidate", "experience", "years", "year", "must", "plus"
    ];
}
