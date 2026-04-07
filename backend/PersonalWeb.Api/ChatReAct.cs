// This file executes the act chosen by the action router.
// The router decides intent; this file maps that intent to concrete backend behavior.
public static class ChatReAct
{
    // The current assistant supports three acts.
    private const string RecruiterSummaryAction = "recruiter-summary";
    private const string JobMatchAction = "job-match";
    private const string AiProjectsAction = "ai-projects";

    public static async Task<ChatResponse> RunAsync(string input)
    {
        var message = input.Trim();

        // First step: ask the router which act the model thinks should run.
        var route = await OllamaActionRouter.RouteAsync(message);
        var action = route.Action;

        // Recruiter summary skips JD analysis and goes directly to Ricardo's skill inventory.
        if (action == RecruiterSummaryAction)
        {
            return BuildRecruiterSummary();
        }

        // AI-projects is a focused retrieval path for AI/RAG/LLM-related work.
        if (action == AiProjectsAction)
        {
            return BuildAiProjectsAnswer();
        }

        // Default heavy path: treat the input as a job match problem.
        return await BuildJobMatchAnswer(message);
    }

    public static async Task<ChatDebugResponse> RunDebugAsync(string input)
    {
        var message = input.Trim();

        // The debug path reuses the same routing decision but returns much more
        // detail so we can inspect the pipeline step by step.
        var route = await OllamaActionRouter.RouteAsync(message);
        var action = route.Action;

        if (action == RecruiterSummaryAction)
        {
            var summary = BuildRecruiterSummary();
            return new ChatDebugResponse(
                Input: input,
                Mode: RecruiterSummaryAction,
                RouterMode: route.Mode,
                RouterReason: route.Reason,
                ExtractionMode: "none",
                Requirements: EmptyRequirements(),
                Queries: ["skills library"],
                Sources: summary.Sources,
                Answer: summary.Answer);
        }

        if (action == AiProjectsAction)
        {
            var projects = BuildAiProjectsAnswer();
            return new ChatDebugResponse(
                Input: input,
                Mode: AiProjectsAction,
                RouterMode: route.Mode,
                RouterReason: route.Reason,
                ExtractionMode: "none",
                Requirements: new JobRequirements(
                    RoleTitle: "AI roles",
                    Summary: "Find Ricardo projects related to AI and RAG.",
                    RequiredSkills: ["AI", "RAG"],
                    PreferredSkills: ["LLM", "automation"],
                    Responsibilities: [],
                    Keywords: ["AI", "RAG", "LLM", "automation"]),
                Queries: ["AI", "RAG", "LLM", "automation"],
                Sources: projects.Sources,
                Answer: projects.Answer);
        }

        // In job-match mode we expose the full chain:
        // route -> JD extraction -> query building -> retrieval -> final answer.
        var analysis = await OllamaJobAnalyzer.AnalyzeWithDebugAsync(message);
        var queries = PortfolioRag.BuildQueriesForDebug(analysis.Requirements);
        var rankedSources = PortfolioRag.Search(analysis.Requirements);
        var sources = rankedSources
            .Select(x => new ChatSource(x.Title, x.Text, x.Category))
            .ToList();

        var answer = rankedSources.Count == 0
            ? "I could not find a strong portfolio match yet. Try pasting a fuller JD or asking about RAG, automation, Python, SQL, Power BI, or AI internships."
            : PortfolioRag.BuildAnswer(analysis.Requirements, rankedSources);

        return new ChatDebugResponse(
            Input: input,
            Mode: JobMatchAction,
            RouterMode: route.Mode,
            RouterReason: route.Reason,
            ExtractionMode: analysis.Mode,
            Requirements: analysis.Requirements,
            Queries: queries,
            Sources: sources,
            Answer: answer);
    }

    private static ChatResponse BuildRecruiterSummary()
    {
        // This act answers a recruiter-style "who is Ricardo?" question.
        // It intentionally does not pretend the user pasted a JD.
        var skills = PortfolioRag.GetSkillLibrary();
        var answer = $"Act used: {RecruiterSummaryAction}\n\nRicardo's skill library includes: {string.Join(", ", skills)}";
        var sources = skills.Count > 0
            ? new List<ChatSource> { new("Skill Library", string.Join(", ", skills), "skills") }
            : new List<ChatSource> { new("Portfolio Summary", "Ricardo builds AI products, data pipelines, RAG systems, and automation workflows.", "summary") };

        return new ChatResponse(answer, sources);
    }

    private static ChatResponse BuildAiProjectsAnswer()
    {
        // This act reuses the RAG layer, but seeds it with a fixed AI-oriented
        // requirement profile instead of extracting one from user input.
        var requirements = new JobRequirements(
            RoleTitle: "AI roles",
            Summary: "Find Ricardo projects related to AI and RAG.",
            RequiredSkills: ["AI", "RAG"],
            PreferredSkills: ["LLM", "automation"],
            Responsibilities: [],
            Keywords: ["AI", "RAG", "LLM", "automation"]);

        var rankedSources = PortfolioRag.Search(requirements)
            .Where(source => string.Equals(source.Category, "projects", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (rankedSources.Count == 0)
        {
            return new ChatResponse(
                $"Act used: {AiProjectsAction}\n\nI could not find AI-focused projects yet.",
                [new ChatSource("Portfolio Summary", "Ricardo builds AI products, data pipelines, RAG systems, and automation workflows.", "summary")]);
        }

        var answerLines = rankedSources
            .Select(source => $"- {source.Title}: {source.Text}")
            .ToList();

        var answer = $"Act used: {AiProjectsAction}\n\nProjects that fit AI roles:\n\n" + string.Join("\n", answerLines);
        var sources = rankedSources
            .Select(x => new ChatSource(x.Title, x.Text, x.Category))
            .ToList();

        return new ChatResponse(answer, sources);
    }

    private static async Task<ChatResponse> BuildJobMatchAnswer(string message)
    {
        // This is the most complete path:
        // 1. Extract structured JD requirements
        // 2. Retrieve matching portfolio evidence
        // 3. Return a grounded fit answer
        var requirements = await OllamaJobAnalyzer.AnalyzeAsync(message);
        var rankedSources = PortfolioRag.Search(requirements);

        if (rankedSources.Count == 0)
        {
            return new ChatResponse(
                $"Act used: {JobMatchAction}\n\nI could not find a strong portfolio match yet. Try pasting a fuller JD or asking about RAG, automation, Python, SQL, Power BI, or AI internships.",
                [new ChatSource("Portfolio Summary", "Ricardo builds AI products, data pipelines, RAG systems, and automation workflows.", "summary")]);
        }

        var sources = rankedSources
            .Select(x => new ChatSource(x.Title, x.Text, x.Category))
            .ToList();

        var answer = $"Act used: {JobMatchAction}\n\n{PortfolioRag.BuildAnswer(requirements, rankedSources)}";
        return new ChatResponse(answer, sources);
    }

    private static JobRequirements EmptyRequirements()
    {
        // Used by debug responses for acts that do not perform JD extraction.
        return new JobRequirements(
            RoleTitle: string.Empty,
            Summary: string.Empty,
            RequiredSkills: [],
            PreferredSkills: [],
            Responsibilities: [],
            Keywords: []);
    }
}
