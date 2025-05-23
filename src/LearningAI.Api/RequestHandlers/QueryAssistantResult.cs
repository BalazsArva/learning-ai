namespace LearningAI.Api.RequestHandlers;

public record QueryAssistantResult(
    string AssistantResponse,
    KnowledgebaseDocumentSearchResult SearchResult);
