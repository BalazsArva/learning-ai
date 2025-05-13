using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace LearningAI.Api.Persistence.RavenDb.Indexes;

public class KnowledgebaseDocumentContentVectorIndex : AbstractIndexCreationTask<KnowledgebaseDocumentDbEntity, KnowledgebaseDocumentContentVectorIndex.IndexEntry>
{
    public class IndexEntry
    {
        public RavenVector<float> Vector { get; set; } = default!;
    }

    public KnowledgebaseDocumentContentVectorIndex()
    {
        Map = documents => documents.Select(doc => new IndexEntry { Vector = doc.Embeddings });

        VectorIndexes.Add(
            x => x.Vector,
            new Raven.Client.Documents.Indexes.Vector.VectorOptions
            {
                SourceEmbeddingType = Raven.Client.Documents.Indexes.Vector.VectorEmbeddingType.Single,
                DestinationEmbeddingType = Raven.Client.Documents.Indexes.Vector.VectorEmbeddingType.Single,
                Dimensions = 384, // TODO: Move to config or const
            });
    }
}