using System.Text;
using System.Text.Json.Nodes;

// A portfolio chunk is the smallest retrieval unit in this project.
// We keep a small piece of text plus metadata and its embedding vector.
public record PortfolioChunk(string Id, string Title, string Category, string Text, float[] Embedding);

// This file implements a lightweight local RAG layer:
// - load portfolio data from info.json
// - split it into chunks
// - embed text into fixed vectors
// - retrieve similar chunks
// - build grounded answers
public static class PortfolioRag
{
    // Small fixed vector size for the local hashed embedding approach.
    private const int EmbeddingSize = 256;
    private const float SimilarityThreshold = 0.05f;
    private const int MaxResults = 6;

    // Build the knowledge base once at startup so every chat request can reuse it.
    public static readonly List<PortfolioChunk> Knowledge = LoadKnowledge();

    public static List<PortfolioChunk> Search(JobRequirements requirements)
    {
        // Convert structured requirements into multiple retrieval queries instead
        // of relying on only one combined sentence.
        var queries = BuildQueries(requirements);

        var ranked = queries
            .SelectMany(query => SearchByText(query))
            .GroupBy(chunk => $"{chunk.Category}|{chunk.Title}|{chunk.Text}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(MaxResults)
            .ToList();

        return DeduplicateByTitle(ranked);
    }

    public static List<string> GetSkillLibrary()
    {
        // Recruiter-summary uses this to surface Ricardo's known skills without
        // pretending the user provided a JD.
        return Knowledge
            .Where(chunk => string.Equals(chunk.Category, "skills", StringComparison.OrdinalIgnoreCase))
            .SelectMany(chunk => chunk.Text.Split([',', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Where(value => !value.Equals("skills", StringComparison.OrdinalIgnoreCase))
            .Where(value => !value.Contains("Programming Languages", StringComparison.OrdinalIgnoreCase))
            .Where(value => !value.Contains("Networking Fundamentals", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToList();
    }

    public static List<string> BuildQueriesForDebug(JobRequirements requirements)
    {
        // Debug runner uses this to show the actual retrieval inputs.
        return BuildQueries(requirements).ToList();
    }

    public static string BuildAnswer(JobRequirements requirements, List<PortfolioChunk> items)
    {
        // Current answer generation is template-based rather than a second LLM pass.
        // That keeps the output deterministic while we debug retrieval quality.
        var builder = new StringBuilder();

        builder.AppendLine("Ricardo looks like a strong fit based on the job requirements I extracted and the closest portfolio evidence.");



        var topSkills = requirements.RequiredSkills
            .Concat(requirements.PreferredSkills)
            .Take(8)
            .ToList();

        if (topSkills.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine($"Key requirements I detected: {string.Join(", ", topSkills)}");
        }

        builder.AppendLine();
        builder.AppendLine("Best matching evidence:");

        foreach (var item in items)
        {
            builder.AppendLine($"- {item.Title}: {item.Text}");
        }

        return builder.ToString().Trim();
    }

    private static IEnumerable<string> BuildQueries(JobRequirements requirements)
    {
        // Spread the retrieval across role title, summary, skills, keywords, and
        // responsibilities so the search can match from multiple angles.
        var queries = new List<string>();

        if (!string.IsNullOrWhiteSpace(requirements.RoleTitle))
        {
            queries.Add(requirements.RoleTitle);
        }

        if (!string.IsNullOrWhiteSpace(requirements.Summary))
        {
            queries.Add(requirements.Summary);
        }

        queries.AddRange(requirements.RequiredSkills);
        queries.AddRange(requirements.PreferredSkills);
        queries.AddRange(requirements.Keywords);
        queries.AddRange(requirements.Responsibilities.Take(4));

        return queries
            .Select(query => query.Trim())
            .Where(query => !string.IsNullOrWhiteSpace(query))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<PortfolioChunk> SearchByText(string query)
    {
        // Embed the query text using the same local embedding function as the knowledge chunks.
        var queryEmbedding = BuildEmbedding(query);

        return Knowledge
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
            })
            .Where(x => x.Score > SimilarityThreshold)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Chunk.Title)
            .Take(MaxResults)
            .Select(x => x.Chunk)
            .ToList();
    }

    private static List<PortfolioChunk> DeduplicateByTitle(List<PortfolioChunk> items)
    {
        // Retrieval can hit multiple chunks from the same project or role.
        // Collapse those so the final answer is easier to read.
        var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var deduped = new List<PortfolioChunk>();

        foreach (var item in items)
        {
            if (!seenTitles.Add($"{item.Category}|{item.Title}"))
            {
                continue;
            }

            deduped.Add(item);

            if (deduped.Count >= 4)
            {
                break;
            }
        }

        return deduped;
    }

    private static List<PortfolioChunk> LoadKnowledge()
    {
        // The portfolio knowledge base lives in a local JSON file instead of a database.
        var path = Path.Combine(AppContext.BaseDirectory, "Properties", "info.json");
        var json = File.ReadAllText(path);
        var root = JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException("Could not parse portfolio info.json");

        var chunks = new List<PortfolioChunk>();
        var index = 0;

        foreach (var section in root)
        {
            if (section.Value is not JsonArray array)
            {
                continue;
            }

            // Each top-level section can produce multiple chunks:
            // summary chunks, detail chunks, and skill chunks.
            foreach (var item in array.OfType<JsonObject>())
            {
                var title = ResolveTitle(item, section.Key, index);
                var summaryText = BuildSummaryText(item, title);

                AddChunk(chunks, $"{section.Key}-{index}-summary", title, section.Key, summaryText);
                AddDetailChunks(chunks, section.Key, index, title, item);
                AddSkillChunk(chunks, section.Key, index, title, item);

                index++;
            }
        }

        AddChunk(
            chunks,
            "portfolio-summary",
            "Ricardo Summary",
            "summary",
            "Ricardo Gao builds practical AI products, data pipelines, RAG systems, analytics workflows, and automation tools."
        );

        return chunks
            .GroupBy(chunk => $"{chunk.Category}|{chunk.Title}|{chunk.Text}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static string ResolveTitle(JsonObject item, string sectionName, int index)
    {
        // Different sections use different fields, so title resolution is flexible.
        return item["title"]?.GetValue<string>()
            ?? item["school"]?.GetValue<string>()
            ?? item["company"]?.GetValue<string>()
            ?? $"{sectionName}-{index + 1}";
    }

    private static string BuildSummaryText(JsonObject item, string title)
    {
        // Summary chunks combine the high-signal metadata for one record.
        var headerParts = new[]
        {
            title,
            item["company"]?.GetValue<string>(),
            item["school"]?.GetValue<string>(),
            item["major"]?.GetValue<string>(),
            item["location"]?.GetValue<string>(),
            item["start_date"]?.GetValue<string>(),
            item["end_date"]?.GetValue<string>()
        }
        .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(" | ", headerParts);
    }

    private static void AddDetailChunks(List<PortfolioChunk> chunks, string sectionName, int index, string title, JsonObject item)
    {
        // Details usually carry the strongest evidence, so each bullet becomes its own chunk.
        if (item["details"] is not JsonArray details)
        {
            return;
        }

        var detailIndex = 0;
        foreach (var detail in details)
        {
            var detailText = detail?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(detailText))
            {
                continue;
            }

            AddChunk(chunks, $"{sectionName}-{index}-detail-{detailIndex}", title, sectionName, detailText);
            detailIndex++;
        }
    }

    private static void AddSkillChunk(List<PortfolioChunk> chunks, string sectionName, int index, string title, JsonObject item)
    {
        // Skill sections are flattened into a single skill-list chunk.
        if (item["skills"] is not JsonArray skills)
        {
            return;
        }

        var skillText = string.Join(", ",
            skills
                .Select(skill => skill?.GetValue<string>())
                .Where(skill => !string.IsNullOrWhiteSpace(skill)));

        AddChunk(chunks, $"{sectionName}-{index}-skills", title, sectionName, $"{title}: {skillText}");
    }

    private static void AddChunk(List<PortfolioChunk> chunks, string id, string title, string category, string text)
    {
        // Centralized chunk creation so every chunk is embedded consistently.
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        chunks.Add(new PortfolioChunk(id, title, category, text.Trim(), BuildEmbedding(text)));
    }

    private static float[] BuildEmbedding(string text)
    {
        // This is a lightweight local embedding approximation:
        // tokenize -> stable hash bucket -> normalized numeric vector.
        var vector = new float[EmbeddingSize];

        foreach (var token in Tokenize(text))
        {
            var bucket = StableHash(token) % EmbeddingSize;
            vector[bucket] += 1f;
        }

        return Normalize(vector);
    }

    private static float CosineSimilarity(float[] left, float[] right)
    {
        // Because vectors are normalized, the dot product acts like cosine similarity.
        var dot = 0f;
        for (var i = 0; i < EmbeddingSize; i++)
        {
            dot += left[i] * right[i];
        }

        return dot;
    }

    private static float[] Normalize(float[] vector)
    {
        // Normalization keeps longer chunks from dominating just because they contain more tokens.
        var magnitude = MathF.Sqrt(vector.Sum(value => value * value));
        if (magnitude <= 0f)
        {
            return vector;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] /= magnitude;
        }

        return vector;
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        // Very simple tokenization is enough for the current lightweight retriever.
        var chars = input
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ')
            .ToArray();

        return chars
            .AsSpan()
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => term.Length > 1);
    }

    private static int StableHash(string value)
    {
        // Stable local hash so the same token always lands in the same bucket.
        unchecked
        {
            var hash = 23;
            foreach (var ch in value)
            {
                hash = (hash * 31) + ch;
            }

            return Math.Abs(hash);
        }
    }
}
