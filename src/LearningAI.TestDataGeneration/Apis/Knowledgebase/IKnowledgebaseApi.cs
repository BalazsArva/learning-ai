using Refit;

namespace LearningAI.TestDataGeneration.Apis.Knowledgebase;

public interface IKnowledgebaseApi
{
    [Post("/api/knowledgebase/documents")]
    Task<IApiResponse<CreateKnowledgebaseDocumentResponse>> CreateDocumentAsync(
        CreateKnowledgebaseDocumentRequest request,
        CancellationToken cancellationToken = default);
}