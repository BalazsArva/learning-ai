using LearningAI.Api.Persistence;
using Microsoft.Extensions.AI;

namespace LearningAI.Api.RequestHandlers;

public class CreateDocumentRequestHandler(
    IKnowledgebaseDocumentRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<CreateDocumentRequestHandler> logger) : ICreateDocumentRequestHandler
{
    public async Task<string> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        var contentsEmbedding = await embeddingGenerator.GenerateVectorAsync(request.Contents, cancellationToken: cancellationToken);
        var id = Guid.NewGuid().ToString();

        await repository.SaveDocumentAsync(new(id, request.Title, request.Contents, contentsEmbedding), cancellationToken);

        return id;
    }
}