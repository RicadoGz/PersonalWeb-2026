// Legacy debug helper kept around for quick direct pipeline inspection.
// The main debug flow now goes through ChatReAct.RunDebugAsync, but this file
// still shows a direct "extraction -> retrieval -> answer" chain.
public static class ChatDebugPipeline
{
    public static async Task<ChatDebugResponse> RunAsync(string input)
    {
        // Bypass action routing and inspect only the JD extraction + RAG path.
        var analysis = await OllamaJobAnalyzer.AnalyzeWithDebugAsync(input);
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
            Mode: "legacy-debug",
            RouterMode: "legacy-debug",
            RouterReason: "Legacy debug pipeline path.",
            ExtractionMode: analysis.Mode,
            Requirements: analysis.Requirements,
            Queries: queries,
            Sources: sources,
            Answer: answer);
    }
}
