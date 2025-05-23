namespace LearningAI.Api.RequestHandlers;

public record QueryAssistantResult(
    string AssistantResponse,
    IReadOnlyCollection<KnowledgebaseDocumentSearchResult> SearchResults);
