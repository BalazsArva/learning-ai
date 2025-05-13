using LearningAI.Api.Persistence.RavenDb.Indexes;
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

    public async Task<IReadOnlyCollection<KnowledgebaseDocumentDbEntity>> SearchDocumentsByContentEmbeddingAsync(ReadOnlyMemory<float> embeddings, CancellationToken cancellationToken)
    {
        using var session = documentStore.OpenAsyncSession();

        var vector = new RavenVector<float>(embeddings.ToArray());

        var results = await session
            .Query<KnowledgebaseDocumentContentVectorIndex.IndexEntry, KnowledgebaseDocumentContentVectorIndex>()
            .VectorSearch(
                x => x.WithField(doc => doc.Vector),
                x => x.ByEmbedding(vector))
            .ProjectInto<KnowledgebaseDocumentDbEntity>()
            .Take(10)
            .ToListAsync();

        return results;
    }
}