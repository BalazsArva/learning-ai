using LearningAI.Api.Persistence.RavenDb.Indexes;
using Raven.Client.Documents;

namespace LearningAI.Api.Persistence.RavenDb;

public class KnowledgebaseDocumentRepository(IDocumentStore documentStore) : IKnowledgebaseDocumentRepository
{
    public async Task SaveDocumentAsync(
        KnowledgebaseDocument document,
        CancellationToken cancellationToken)
    {
        using var session = documentStore.OpenAsyncSession();

        var entity = new KnowledgebaseDocumentDbEntity
        {
            Id = document.Id,
            Title = document.Title,
            Contents = document.Contents,
            Embeddings = new RavenVector<float>(document.Embeddings.ToArray()),
        };

        await session.StoreAsync(entity, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<KnowledgebaseDocument>> SearchDocumentsByContentEmbeddingAsync(
        ReadOnlyMemory<float> embeddings,
        int documentCount = 3,
        CancellationToken cancellationToken = default)
    {
        using var session = documentStore.OpenAsyncSession();

        var vector = new RavenVector<float>(embeddings.ToArray());
        var dbEntities = await session
            .Query<KnowledgebaseDocumentContentVectorIndex.IndexEntry, KnowledgebaseDocumentContentVectorIndex>()
            .VectorSearch(
                embeddingFieldFactory: x => x.WithField(doc => doc.Vector),
                embeddingValueFactory: x => x.ByEmbedding(vector),
                minimumSimilarity: 0.7f)
            .ProjectInto<KnowledgebaseDocumentDbEntity>()
            .Take(documentCount)
            .ToListAsync(cancellationToken);

        return dbEntities.Select(x => new KnowledgebaseDocument(x.Id, x.Title, x.Contents, x.Embeddings.ToArray())).ToList();
    }

    public async Task<IReadOnlyCollection<KnowledgebaseDocument>> SearchDocumentsByKeywordsAsync(
        string searchTerm,
        int documentCount = 3,
        CancellationToken cancellationToken = default)
    {
        using var session = documentStore.OpenAsyncSession();

        var dbEntities = await session
            .Query<KnowledgebaseDocumentDbEntity>()
            .Search(x => x.Contents, searchTerm, options: SearchOptions.Or)
            .OrderByScore()
            .Take(documentCount)
            .ToListAsync(cancellationToken);

        return dbEntities.Select(x => new KnowledgebaseDocument(x.Id, x.Title, x.Contents, x.Embeddings.ToArray())).ToList();
    }
}