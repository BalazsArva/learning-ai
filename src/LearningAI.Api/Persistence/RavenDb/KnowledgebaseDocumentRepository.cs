using Raven.Client.Documents;

namespace LearningAI.Api.Persistence.RavenDb;

public class KnowledgebaseDocumentRepository(IDocumentStore documentStore) : IKnowledgebaseDocumentRepository
{
    public async Task SaveDocumentAsync(KnowledgebaseDocument document, CancellationToken cancellationToken)
    {
        using var session = documentStore.OpenAsyncSession();

        var entity = new KnowledgebaseDocumentDbEntity
        {
            Id = document.Id,
            Title = document.Title,
            Contents = document.Contents,
            Embeddings = new(document.Embeddings.ToArray()),
        };

        await session.StoreAsync(entity, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);
    }
}