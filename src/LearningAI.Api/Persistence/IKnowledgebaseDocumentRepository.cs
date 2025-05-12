namespace LearningAI.Api.Persistence;

public interface IKnowledgebaseDocumentRepository
{
    Task SaveDocumentAsync(KnowledgebaseDocument document, CancellationToken cancellationToken);
}

public record KnowledgebaseDocument(
    string Id,
    string Title,
    string Contents,
    ReadOnlyMemory<float> Embeddings);