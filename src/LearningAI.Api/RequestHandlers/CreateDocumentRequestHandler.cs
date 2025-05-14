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
        var id = Guid.NewGuid().ToString();
        var contentsEmbedding = await embeddingGenerator.GenerateVectorAsync(request.Contents, cancellationToken: cancellationToken);

        await repository.SaveDocumentAsync(new(id, request.Title, request.Contents, contentsEmbedding), cancellationToken);

        logger.LogInformation("Created document with Id={Id} Title={Title}", id, request.Title);

        return id;
    }
}