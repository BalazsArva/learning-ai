using LearningAI.Api.Persistence;
using LearningAI.Api.Utilities;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.AIFunctions;

public class KnowledgebaseTools(
    IKnowledgebaseDocumentRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IUriProvider uriProvider,
    ILogger<KnowledgebaseTools> logger) : IKnowledgebaseTools
{
    public async Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        string query,
        CancellationToken cancellationToken)
    {
        // TODO: Consider Markdown chunking: https://ravendb.net/docs/article-page/7.0/csharp/ai-integration/generating-embeddings/embeddings-generation-task
        // If the "theoretically required" document is less focused, then the embedding of the document as a whole may be too different from the embedding of
        // the query.
        // To solve this, we can:
        // - try to chunk the document so that among the chucks' embeddings there will be a much better fitting candidate. Note that in this case multiple
        //   embeddings of the same document may cause the document to appear in the results, so the same document may be present multiple times. We need to
        //   select distinct ones so we avoid sending duplicate document contents and causing extra costs.
        // - lower the similarity score but that risks introducing too dissimilar documents in other cases. The LLM should be able to realize those dissimilar
        //   docs don't contain the wanted info but the token usage will still be high when sending over the irrelevant documents
        // - don't set a threshold but always only use at most 1 document only?
        logger.LogInformation("Searching for documents by query '{Query}'", query);

        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(query, cancellationToken: cancellationToken);
        var documents = await repository.SearchDocumentsByContentEmbeddingAsync(queryEmbedding, cancellationToken: cancellationToken);

        logger.LogInformation("Search by query {Query} yielded {MatchCount} results.", query, documents.Count);

        return [.. documents.DistinctBy(d => d.Id).Select(d => d.Contents)];
    }

    public async Task<IReadOnlyCollection<string>> SearchDocumentsByQueryKeywordsAsync(
        string queryKeywords,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching for documents by query keywords {QueryKeywords}", queryKeywords);

        var documents = await repository.SearchDocumentsByKeywordsAsync(queryKeywords, cancellationToken: cancellationToken);

        logger.LogInformation("Search by query keywords {QueryKeywords} yielded {MatchCount} results.", queryKeywords, documents.Count);

        return [.. documents.Select(d => d.Contents)];
    }

    public async Task<IReadOnlyCollection<SearchDocumentToolResult>> SearchDocumentsAsync(
        string query,
        string[] queryKeywords,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Searching for documents by query {Query} and query keywords {QueryKeywords}",
            query,
            queryKeywords);

        var keywordsSearchTask = repository.SearchDocumentsByKeywordsAsync(string.Join(" ", queryKeywords), cancellationToken: cancellationToken);
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(query, cancellationToken: cancellationToken);
        var semanticSearchTask = repository.SearchDocumentsByContentEmbeddingAsync(queryEmbedding, cancellationToken: cancellationToken);

        await Task.WhenAll(keywordsSearchTask, semanticSearchTask);

        var keywordsSearchResult = await keywordsSearchTask;
        var semanticSearchResult = await semanticSearchTask;

        // Note: the logic below is based on "gut feeling" that when there are strongly matching documents, they will show up
        // in both semantic- and keywords-based search results. When there is any doc found in both, we can disregard anything
        // else that's only present in one of the results. This way we don't needlessly send too many tokens to the LLM but can
        // still fall back to using every partial result if there isn't a strong-enough match.
        var resultsFoundInBoth = keywordsSearchResult.IntersectBy(semanticSearchResult.Select(x => x.Id).ToHashSet(), x => x.Id);
        if (resultsFoundInBoth.Any())
        {
            var strongMatches = resultsFoundInBoth
                .DistinctBy(x => x.Id)
                .Select(x => new SearchDocumentToolResult(x.Id, x.Title, x.Contents, uriProvider.GetUriForKnowledgebaseDocumentByTitle(x.Title)))
                .ToList();

            logger.LogInformation(
                "Searching by query {Query} and query keywords {QueryKeywords} yielded {MatchCount} strongly matching documents.",
                query,
                queryKeywords,
                strongMatches.Count);

            return strongMatches;
        }

        var weakMatches = keywordsSearchResult
            .Concat(semanticSearchResult)
            .DistinctBy(x => x.Id)
            .Select(x => new SearchDocumentToolResult(x.Id, x.Title, x.Contents, uriProvider.GetUriForKnowledgebaseDocumentByTitle(x.Title)))
            .ToList();

        logger.LogInformation(
            "Searching by query {Query} and query keywords {QueryKeywords} yielded {MatchCountByQuery} and {MatchCountByKeywords} weakly matching documents.",
            query,
            queryKeywords,
            semanticSearchResult.Count,
            keywordsSearchResult.Count);

        return weakMatches;
    }

    public async Task<IReadOnlyCollection<CalendarEntry>> GetCalendarForNextNDaysAsync(int daysOffset, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var result = new List<CalendarEntry>(daysOffset + 1);
        var sign = Math.Sign(daysOffset);

        for (var i = 0; i <= daysOffset; ++i)
        {
            var date = today.AddDays(sign * i);

            result.Add(new CalendarEntry(date.DayOfWeek.ToString(), date));
        }

        return result;
    }
}