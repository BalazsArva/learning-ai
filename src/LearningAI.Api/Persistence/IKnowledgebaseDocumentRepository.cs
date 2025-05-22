using LearningAI.Api.Persistence.RavenDb;

namespace LearningAI.Api.Persistence;

public interface IKnowledgebaseDocumentRepository
{
    Task SaveDocumentAsync(
        KnowledgebaseDocument document,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<KnowledgebaseDocumentDbEntity>> SearchDocumentsByContentEmbeddingAsync(
        ReadOnlyMemory<float> embeddings,
        int documentCount = 3,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<KnowledgebaseDocumentDbEntity>> SearchDocumentsByKeywordsAsync(
        string searchTerm,
        int documentCount = 3,
        CancellationToken cancellationToken = default);
}

public record KnowledgebaseDocument(
    string Id,
    string Title,
    string Contents,
    ReadOnlyMemory<float> Embeddings);