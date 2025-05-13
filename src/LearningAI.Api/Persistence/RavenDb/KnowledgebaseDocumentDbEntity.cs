namespace LearningAI.Api.Persistence.RavenDb;

public class KnowledgebaseDocumentDbEntity
{
    public string Id { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Contents { get; set; } = default!;

    public float[] Embeddings { get; set; } = default!;
}