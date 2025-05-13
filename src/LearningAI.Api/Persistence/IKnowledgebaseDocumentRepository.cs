using LearningAI.Api.Persistence.RavenDb;

namespace LearningAI.Api.Persistence;

public interface IKnowledgebaseDocumentRepository
{
    Task SaveDocumentAsync(
        KnowledgebaseDocument document,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<KnowledgebaseDocumentDbEntity>> SearchDocumentsByContentEmbeddingAsync(
        ReadOnlyMemory<float> embeddings,
        CancellationToken cancellationToken);
}

public record KnowledgebaseDocument(
    string Id,
    string Title,
    string Contents,
    ReadOnlyMemory<float> Embeddings);