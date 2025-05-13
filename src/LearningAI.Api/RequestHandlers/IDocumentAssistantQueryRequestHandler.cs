namespace LearningAI.Api.RequestHandlers;

public interface IDocumentAssistantQueryRequestHandler
{
    Task<QueryAssistantResult> QueryAssistantAsync(QueryAssistantRequest request, CancellationToken cancellationToken);
}
