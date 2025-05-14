using System.Web;
using LearningAI.Api.Persistence;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.AIFunctions;

public class KnowledgebaseTools(
    IKnowledgebaseDocumentRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<KnowledgebaseTools> logger) : IKnowledgebaseTools
{
    public async Task<IReadOnlyCollection<string>> SearchDocumentsByContentSemanticsAsync(
        string query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching for documents by query '{Query}'", query);

        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(query, cancellationToken: cancellationToken);
        var documents = await repository.SearchDocumentsByContentEmbeddingAsync(queryEmbedding, cancellationToken);

        logger.LogInformation("Search by query {Query} yielded {MatchCount} results.", query, documents.Count);

        return [.. documents.Select(d => d.Contents)];
    }

    public async Task<string> GetUriForDocumentAsync(string documentTitle, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting URL for document '{DocumentTitle}'", documentTitle);

        var encodedTitle = HttpUtility.UrlEncode(documentTitle);

        // TODO: Return non-mock data
        return $"http://localhost:5011/api/knowledgebase/documents/{encodedTitle}";
    }
}