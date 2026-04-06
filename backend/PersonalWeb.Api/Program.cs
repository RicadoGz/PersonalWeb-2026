var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Hello from ASP.NET Core"
    });
});

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

app.MapGet("/api/auth/me", () =>
{
    return Results.Ok(new
    {
        authenticated = false
    });
});

app.MapPost("/api/chat", (ChatRequest request) =>
{
    var question = (request.Message ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(question))
    {
        return Results.BadRequest(new
        {
            error = "Message is required"
        });
    }

    var rankedSources = PortfolioRag.Knowledge
        .Select(item => new
        {
            Item = item,
            Score = PortfolioRag.CalculateScore(question, item)
        })
        .Where(x => x.Score > 0)
        .OrderByDescending(x => x.Score)
        .ThenBy(x => x.Item.Title)
        .Take(3)
        .ToList();

    if (rankedSources.Count == 0)
    {
        return Results.Ok(new ChatResponse(
            Answer: "I could not find a strong match in Ricardo's portfolio yet. Try asking about RAG, automation, Power BI, Python, SQL, internships, or project fit.",
            Sources:
            [
                new ChatSource("Portfolio Summary", "Ricardo builds AI products, data pipelines, RAG systems, and automation workflows.", "summary")
            ]
        ));
    }

    var sources = rankedSources
        .Select(x => new ChatSource(x.Item.Title, x.Item.Snippet, x.Item.Category))
        .ToList();

    var answer = PortfolioRag.BuildAnswer(question, rankedSources.Select(x => x.Item).ToList());

    return Results.Ok(new ChatResponse(answer, sources));
});

app.Run();

public record LoginRequest(string Username, string Password);

public record ChatRequest(string Message);

public record ChatResponse(string Answer, List<ChatSource> Sources);

public record ChatSource(string Title, string Snippet, string Category);

public record KnowledgeItem(string Title, string Category, string Snippet, string Tags);

public static class PortfolioRag
{
    public static readonly List<KnowledgeItem> Knowledge =
    [
        new(
            "Ricardo Summary",
            "summary",
            "Ricardo Gao is a Waterloo-based software engineering student who builds practical AI products, data pipelines, RAG systems, and automation workflows.",
            "ricardo portfolio ai developer data automation rag full-stack"
        ),
        new(
            "Open Claw-Inspired Market Sensing Agent",
            "project",
            "Built an AI agent to gather and organize market signals from reviews, surveys, and competitor content using Python, SQL, and Power BI.",
            "python sql power bi ai agent market sensing analytics"
        ),
        new(
            "AI-Powered n8n Automation for Maintenance Request Triage",
            "project",
            "Built an n8n workflow for request intake, AI classification, contractor match, notification, and logging with Anthropic API integration.",
            "n8n anthropic api automation workflow triage reliability"
        ),
        new(
            "AI-Powered AuditRAG",
            "project",
            "Built a source-grounded RAG system with chunking, embeddings, and retrieval to improve traceability and reduce unsupported answers.",
            "rag embeddings retrieval ai research assistant finance filings news"
        ),
        new(
            "Stock Daily Report Automation",
            "project",
            "Built a UIPath automation to retrieve daily market data, improve retry logic, and automate Outlook report distribution.",
            "uipath automation reporting market data workflow"
        ),
        new(
            "Stealth Startup Experience",
            "experience",
            "Designed and delivered a full-stack web platform using Django and REST, plus a chatbot RAG system on a 27K dataset with grounded answer generation.",
            "django rest full-stack chatbot rag ollama startup product"
        ),
        new(
            "PAL Leader Experience",
            "experience",
            "Led 20 debugging sessions and helped students significantly improve quiz and assignment performance in C programming.",
            "teaching debugging c programming mentoring communication"
        ),
        new(
            "GDG Waterloo Leadership",
            "experience",
            "Built Python automation to reduce setup time by 75% and helped attract 300 attendees through technical workshops.",
            "leadership python automation workshops flask selenium community"
        ),
        new(
            "Core Skills",
            "skills",
            "Strong skills in Python, SQL, Power BI, machine learning, generative AI, LLM, RAG, prompt engineering, Pandas, NumPy, scikit-learn, OpenAI API, Anthropic API, UIPath, n8n, Django, and FastAPI.",
            "skills python sql power bi machine learning llm rag prompt engineering pandas numpy scikit-learn openai anthropic uipath n8n django fastapi"
        )
    ];

    public static int CalculateScore(string question, KnowledgeItem item)
    {
        var queryTerms = Tokenize(question);
        var contentTerms = Tokenize($"{item.Title} {item.Category} {item.Snippet} {item.Tags}");

        return queryTerms.Count(term => contentTerms.Contains(term));
    }

    public static string BuildAnswer(string question, List<KnowledgeItem> items)
    {
        var lowerQuestion = question.ToLowerInvariant();
        var opening = lowerQuestion.Contains("fit") || lowerQuestion.Contains("job") || lowerQuestion.Contains("jd") || lowerQuestion.Contains("role")
            ? "Ricardo looks like a strong fit based on the parts of his portfolio that overlap with your request."
            : "Here is the most relevant information I found in Ricardo's portfolio.";

        var bullets = items
            .Select(item => $"- {item.Title}: {item.Snippet}")
            .ToList();

        return $"{opening}\n\n{string.Join("\n", bullets)}";
    }

    private static HashSet<string> Tokenize(string input)
    {
        var chars = input
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ')
            .ToArray();

        return chars
            .AsSpan()
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => term.Length > 1)
            .ToHashSet();
    }
}
