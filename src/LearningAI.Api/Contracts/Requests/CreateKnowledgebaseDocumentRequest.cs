namespace LearningAI.Api.Contracts.Requests;

public record CreateKnowledgebaseDocumentRequest(
    string Title,
    string Content);