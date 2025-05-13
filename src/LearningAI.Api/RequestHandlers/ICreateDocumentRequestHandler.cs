namespace LearningAI.Api.RequestHandlers;

public interface ICreateDocumentRequestHandler
{
    Task<string> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken);
}