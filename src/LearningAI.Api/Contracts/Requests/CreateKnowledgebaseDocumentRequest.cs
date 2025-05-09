namespace LearningAI.Api.Contracts.Requests;

public record CreateKnowledgebaseDocumentRequest(
    string Title,
    Guid FolderId,
    string Content);